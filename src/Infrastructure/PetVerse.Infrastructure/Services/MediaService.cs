using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetVerse.Core.DTOs.Media;
using PetVerse.Core.Entities;
using PetVerse.Core.Exceptions;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Infrastructure.Services;

/// <summary>
/// 媒体服务实现
/// </summary>
public class MediaService : IMediaService
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly ILogger<MediaService> _logger;
    private readonly HashSet<string> _allowedExtensions;
    private readonly long _maxFileSize;

    public MediaService(
        ApplicationDbContext context,
        IStorageService storageService,
        IConfiguration configuration,
        ILogger<MediaService> logger)
    {
        _context = context;
        _storageService = storageService;
        _logger = logger;

        // 从配置读取允许的文件类型和大小限制
        var allowedExts = configuration["Media:AllowedExtensions"] ?? ".jpg,.jpeg,.png,.gif,.mp4,.mov,.avi";
        _allowedExtensions = new HashSet<string>(allowedExts.Split(',', StringSplitOptions.RemoveEmptyEntries));
        
        var maxSizeStr = configuration["Media:MaxFileSize"] ?? "10485760"; // 默认10MB
        _maxFileSize = long.Parse(maxSizeStr);
    }

    public async Task<MediaResponse> UploadMediaAsync(string fileName, string contentType, Stream stream, ulong postId, ulong userId, ushort displayOrder = 0)
    {
        // 验证文件
        if (!ValidateFileType(fileName))
        {
            throw new DomainException($"不支持的文件类型: {Path.GetExtension(fileName)}");
        }

        // 验证帖子存在性和权限
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new NotFoundException($"帖子ID {postId} 不存在");
        }

        if (post.UserId != userId)
        {
            throw new UnauthorizedAccessException("无权限操作此帖子的媒体文件");
        }

        try
        {
            // 上传到存储服务
            var folder = $"posts/{postId:D}/media";
            var (url, key) = await _storageService.UploadFileAsync(fileName, contentType, stream, folder);

            // 创建媒体记录
            var media = new PostMedia
            {
                PostId = postId,
                MediaType = DetermineMediaType(contentType),
                MimeType = contentType,
                OriginalName = fileName,
                StorageKey = key,
                UrlPath = url,
                DisplayOrder = displayOrder,
                Status = 1
            };

            _context.PostMedias.Add(media);
            await _context.SaveChangesAsync();

            // 更新帖子的媒体数量
            post.MediaCount = (byte)(await _context.PostMedias.CountAsync(m => m.PostId == postId && m.Status == 1));
            await _context.SaveChangesAsync();

            _logger.LogInformation("媒体文件上传成功: ID={MediaId}, PostId={PostId}, Key={Key}", media.Id, postId, key);

            return MapToMediaResponse(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件上传失败: PostId={PostId}, FileName={FileName}", postId, fileName);
            throw new InvalidOperationException($"媒体文件上传失败: {ex.Message}");
        }
    }

    // 批量上传功能可以通过控制器层面实现，此处暂不实现
    // public async Task<List<MediaResponse>> UploadMediasAsync(IEnumerable<IFormFile> files, ulong postId, ulong userId)
    // {
    //     var results = new List<MediaResponse>();
    //     var displayOrder = 0;
    //
    //     foreach (var file in files)
    //     {
    //         var result = await UploadMediaAsync(file, postId, userId, (ushort)displayOrder);
    //         results.Add(result);
    //         displayOrder++;
    //     }
    //
    //     return results;
    // }

    public async Task<bool> DeleteMediaAsync(ulong mediaId, ulong userId)
    {
        var media = await _context.PostMedias
            .Include(m => m.Post)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
        {
            throw new NotFoundException($"媒体ID {mediaId} 不存在");
        }

        if (media.Post?.UserId != userId)
        {
            throw new UnauthorizedAccessException("无权限删除此媒体文件");
        }

        try
        {
            // 从存储服务删除文件
            await _storageService.DeleteFileAsync(media.StorageKey);

            // 软删除数据库记录
            media.Status = 0; // 0表示已删除
            await _context.SaveChangesAsync();

            // 更新帖子的媒体数量
            if (media.Post != null)
            {
                media.Post.MediaCount = (byte)(await _context.PostMedias.CountAsync(m => m.PostId == media.Post.Id && m.Status == 1));
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("媒体文件删除成功: ID={MediaId}, Key={Key}", mediaId, media.StorageKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件删除失败: ID={MediaId}", mediaId);
            return false;
        }
    }

    public async Task<int> DeleteMediasAsync(IEnumerable<ulong> mediaIds, ulong userId)
    {
        var successCount = 0;
        
        foreach (var mediaId in mediaIds)
        {
            if (await DeleteMediaAsync(mediaId, userId))
            {
                successCount++;
            }
        }
        
        return successCount;
    }

    public async Task<List<MediaResponse>> GetPostMediasAsync(ulong postId)
    {
        var medias = await _context.PostMedias
            .Where(m => m.PostId == postId && m.Status == 1)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        return medias.Select(MapToMediaResponse).ToList();
    }

    public async Task<MediaResponse> GetMediaAsync(ulong mediaId)
    {
        var media = await _context.PostMedias.FindAsync(mediaId);
        
        if (media == null || media.Status != 1)
        {
            throw new NotFoundException($"媒体ID {mediaId} 不存在或已被删除");
        }

        return MapToMediaResponse(media);
    }

    public async Task<MediaResponse> UpdateMediaAsync(ulong mediaId, UpdateMediaRequest request, ulong userId)
    {
        var media = await _context.PostMedias
            .Include(m => m.Post)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
        {
            throw new NotFoundException($"媒体ID {mediaId} 不存在");
        }

        if (media.Post?.UserId != userId)
        {
            throw new UnauthorizedAccessException("无权限更新此媒体文件");
        }

        // 更新媒体信息
        if (request.DisplayOrder.HasValue)
        {
            media.DisplayOrder = request.DisplayOrder.Value;
        }

        if (!string.IsNullOrEmpty(request.OriginalName))
        {
            media.OriginalName = request.OriginalName;
        }

        media.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("媒体文件更新成功: ID={MediaId}", mediaId);

        return MapToMediaResponse(media);
    }

    public bool ValidateFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    public bool ValidateFileSize(long fileSize)
    {
        return fileSize <= _maxFileSize;
    }

    private MediaType DetermineMediaType(string contentType)
    {
        if (contentType.StartsWith("image/"))
            return MediaType.Image;
        if (contentType.StartsWith("video/"))
            return MediaType.Video;
        if (contentType.StartsWith("audio/"))
            return MediaType.Audio;
        return MediaType.Other;
    }

    private MediaResponse MapToMediaResponse(PostMedia media)
    {
        return new MediaResponse
        {
            Id = media.Id,
            PostId = media.PostId,
            MediaType = media.MediaType,
            MimeType = media.MimeType,
            OriginalName = media.OriginalName,
            Url = media.UrlPath ?? string.Empty,
            StorageKey = media.StorageKey,
            DisplayOrder = media.DisplayOrder,
            Status = media.Status,
            CreatedAt = media.CreatedAt,
            UpdatedAt = media.UpdatedAt
        };
    }
}