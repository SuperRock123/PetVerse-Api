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

    public async Task<MediaResponse> UploadMediaAsync(string fileName, string contentType, Stream stream, string? urlPath, ulong userId)
    {
        // 验证文件
        if (!ValidateFileType(fileName))
        {
            throw new DomainException($"不支持的文件类型: {Path.GetExtension(fileName)}");
        }

        try
        {
            // 上传到存储服务
            var folder = $"media/{userId:D}";
            var (url, key) = await _storageService.UploadFileAsync(fileName, contentType, stream, folder);

            // 创建媒体记录
            var media = new MediaResource
            {
                UserId = userId,
                MediaType = DetermineMediaType(contentType),
                MimeType = contentType,
                OriginalName = fileName,
                StorageKey = key,
                UrlPath = urlPath, // 传入的urlPath，可能为null
                Status = 1
            };

            _context.MediaResources.Add(media);
            await _context.SaveChangesAsync();

            _logger.LogInformation("媒体文件上传成功: ID={MediaId}, UserId={UserId}, Key={Key}", media.Id, userId, key);

            return MapToMediaResponse(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件上传失败: UserId={UserId}, FileName={FileName}", userId, fileName);
            throw new InvalidOperationException($"媒体文件上传失败: {ex.Message}");
        }
    }

    public async Task<bool> DeleteMediaAsync(ulong mediaId, ulong userId)
    {
        var media = await _context.MediaResources
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
        {
            throw new NotFoundException($"媒体ID {mediaId} 不存在");
        }

        // 验证权限：确保媒体资源属于当前用户
        if (media.UserId != userId)
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

            _logger.LogInformation("媒体文件删除成功: ID={MediaId}, Key={Key}", mediaId, media.StorageKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件删除失败: ID={MediaId}", mediaId);
            return false;
        }
    }

    public async Task<bool> DeleteMediaByStorageKeyAsync(string storageKey, ulong userId)
    {
        var media = await _context.MediaResources
            .FirstOrDefaultAsync(m => m.StorageKey == storageKey && m.UserId == userId);

        if (media == null)
        {
            throw new NotFoundException($"媒体文件不存在或无权限访问: {storageKey}");
        }

        try
        {
            // 从存储服务删除文件
            await _storageService.DeleteFileAsync(storageKey);

            // 软删除数据库记录
            media.Status = 0; // 0表示已删除
            await _context.SaveChangesAsync();

            _logger.LogInformation("媒体文件删除成功: Key={Key}, UserId={UserId}", storageKey, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件删除失败: Key={Key}", storageKey);
            return false;
        }
    }

    public async Task<int> DeleteMediasByStorageKeysAsync(List<string> storageKeys, ulong userId)
    {
        var deletedCount = 0;

        foreach (var storageKey in storageKeys)
        {
            try
            {
                var media = await _context.MediaResources
                    .FirstOrDefaultAsync(m => m.StorageKey == storageKey && m.UserId == userId);

                if (media == null)
                {
                    _logger.LogWarning("媒体文件不存在或无权限访问: Key={Key}, UserId={UserId}", storageKey, userId);
                    continue;
                }

                // 从存储服务删除文件
                await _storageService.DeleteFileAsync(storageKey);

                // 软删除数据库记录
                media.Status = 0; // 0表示已删除
                await _context.SaveChangesAsync();

                _logger.LogInformation("媒体文件删除成功: Key={Key}, UserId={UserId}", storageKey, userId);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "媒体文件删除失败: Key={Key}", storageKey);
                // 继续处理下一个文件
            }
        }

        return deletedCount;
    }

    public async Task<MediaResponse> GetMediaAsync(ulong mediaId)
    {
        var media = await _context.MediaResources.FindAsync(mediaId);
        
        if (media == null || media.Status != 1)
        {
            throw new NotFoundException($"媒体ID {mediaId} 不存在或已被删除");
        }

        return MapToMediaResponse(media);
    }

    public async Task<MediaResponse> UpdateMediaAsync(ulong mediaId, UpdateMediaRequest request, ulong userId)
    {
        var media = await _context.MediaResources
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null)
        {
            throw new NotFoundException($"媒体ID {mediaId} 不存在");
        }

        // 验证权限：确保媒体资源属于当前用户
        if (media.UserId != userId)
        {
            throw new UnauthorizedAccessException("无权限更新此媒体文件");
        }

        // 更新媒体信息
        if (!string.IsNullOrEmpty(request.OriginalName))
        {
            media.OriginalName = request.OriginalName;
        }

        if (request.Meta != null)
        {
            // 这里需要根据实际的Meta类型进行处理
            // 暂时假设Meta是JSON格式
            media.Meta = Newtonsoft.Json.JsonConvert.SerializeObject(request.Meta);
        }

        if (request.Status.HasValue)
        {
            media.Status = request.Status.Value;
        }

        media.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("媒体文件更新成功: ID={MediaId}", mediaId);

        return MapToMediaResponse(media);
    }

    public async Task<List<MediaResponse>> GetUserMediasAsync(ulong userId)
    {
        var medias = await _context.MediaResources
            .Where(m => m.UserId == userId && m.Status == 1)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return medias.Select(MapToMediaResponse).ToList();
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

    private string DetermineMediaType(string contentType)
    {
        if (contentType.StartsWith("image/"))
            return "image";
        if (contentType.StartsWith("video/"))
            return "video";
        if (contentType.StartsWith("audio/"))
            return "audio";
        return "other";
    }

    private string MapMediaTypeFromInt(int mediaTypeInt)
    {
        switch (mediaTypeInt)
        {
            case 0: return "image";
            case 1: return "video";
            case 2: return "audio";
            case 3: return "other";
            default: return "other";
        }
    }

    private int MapMediaTypeToInt(string mediaType)
    {
        switch (mediaType.ToLower())
        {
            case "image": return 0;
            case "video": return 1;
            case "audio": return 2;
            case "other": return 3;
            default: return 3;
        }
    }

    private MediaResponse MapToMediaResponse(MediaResource media)
    {
        object? meta = null;
        if (!string.IsNullOrEmpty(media.Meta))
        {
            try
            {
                meta = Newtonsoft.Json.JsonConvert.DeserializeObject(media.Meta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析媒体元数据失败: ID={MediaId}", media.Id);
            }
        }

        // 尝试将 MediaType 从字符串转换为整数，然后映射为正确的字符串
        string mediaTypeStr = media.MediaType;
        if (int.TryParse(media.MediaType, out int mediaTypeInt))
        {
            mediaTypeStr = MapMediaTypeFromInt(mediaTypeInt);
        }

        return new MediaResponse
        {
            Id = media.Id,
            UserId = media.UserId,
            MediaType = mediaTypeStr,
            MimeType = media.MimeType,
            OriginalName = media.OriginalName,
            StorageKey = media.StorageKey,
            UrlPath = media.UrlPath,
            Meta = meta,
            Status = media.Status,
            CreatedAt = media.CreatedAt,
            UpdatedAt = media.UpdatedAt
        };
    }
}