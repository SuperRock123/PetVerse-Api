using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetVerse.Api.Attributes;
using PetVerse.Api.DTOs.Media;
using PetVerse.Core.DTOs;
using PetVerse.Core.DTOs.Media;
using PetVerse.Core.Interfaces;

namespace PetVerse.Api.Controllers;

/// <summary>
/// 媒体资源管理控制器
/// </summary>
[ApiController]
[Route("api/media")]
[Microsoft.AspNetCore.Authorization.Authorize] // 需要认证
public class MediaController : BaseController
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMediaService mediaService, ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
    }

    /// <summary>
    /// 上传单个媒体文件
    /// </summary>
    /// <param name="request">上传请求</param>
    /// <returns>媒体信息</returns>
    [HttpPost]
    public async Task<IActionResult> UploadMedia([FromForm] UploadMediaRequest request)
    {
        try
        {
            // 获取当前用户ID
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("无效的用户身份");
            }

            using var stream = request.File.OpenReadStream();
            var result = await _mediaService.UploadMediaAsync(
                request.File.FileName, 
                request.File.ContentType, 
                stream, 
                request.PostId, 
                userId, 
                request.DisplayOrder);
            return Success(result, "媒体文件上传成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件上传失败");
            return InternalError("媒体文件上传失败");
        }
    }

    /// <summary>
    /// 批量上传媒体文件
    /// </summary>
    /// <param name="request">批量上传请求</param>
    /// <returns>媒体信息集合</returns>
    [HttpPost("batch")]
    public async Task<IActionResult> UploadMedias([FromForm] UploadMediasRequest request)
    {
        try
        {
            // 获取当前用户ID
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("无效的用户身份");
            }

            var results = new List<MediaResponse>();
            var displayOrder = 0;

            foreach (var file in request.Files)
            {
                using var stream = file.OpenReadStream();
                var result = await _mediaService.UploadMediaAsync(
                    file.FileName, 
                    file.ContentType, 
                    stream, 
                    request.PostId, 
                    userId, 
                    (ushort)displayOrder);
                results.Add(result);
                displayOrder++;
            }
            return Success(results, "媒体文件批量上传成功", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件批量上传失败");
            return InternalError("媒体文件批量上传失败");
        }
    }

    /// <summary>
    /// 删除媒体文件
    /// </summary>
    /// <param name="id">媒体ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedia(ulong id)
    {
        try
        {
            // 获取当前用户ID
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("无效的用户身份");
            }

            var result = await _mediaService.DeleteMediaAsync(id, userId);
            
            if (result)
            {
                return Success<object>(new { deletedCount = 1 }, "媒体文件删除成功");
            }
            else
            {
                return Error("媒体文件删除失败");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件删除失败，ID: {MediaId}", id);
            return InternalError("媒体文件删除失败");
        }
    }

    /// <summary>
    /// 批量删除媒体文件
    /// </summary>
    /// <param name="ids">媒体ID集合</param>
    /// <returns>删除成功的数量</returns>
    [HttpDelete("batch")]
    public async Task<IActionResult> DeleteMedias([FromBody] List<ulong> ids)
    {
        try
        {
            // 获取当前用户ID
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("无效的用户身份");
            }

            var deletedCount = 0;
            foreach (var id in ids)
            {
                if (await _mediaService.DeleteMediaAsync(id, userId))
                {
                    deletedCount++;
                }
            }
            return Success<object>(new { deletedCount }, $"成功删除 {deletedCount} 个媒体文件");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体文件批量删除失败");
            return InternalError("媒体文件批量删除失败");
        }
    }

    /// <summary>
    /// 获取帖子的所有媒体文件
    /// </summary>
    /// <param name="postId">帖子ID</param>
    /// <returns>媒体信息集合</returns>
    [HttpGet("post/{postId}")]
    [AllowAnonymous] // 允许匿名访问
    public async Task<IActionResult> GetPostMedias(ulong postId)
    {
        try
        {
            var results = await _mediaService.GetPostMediasAsync(postId);
            return Success(results, "获取帖子媒体文件成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取帖子媒体文件失败，PostId: {PostId}", postId);
            return InternalError("获取帖子媒体文件失败");
        }
    }

    /// <summary>
    /// 获取媒体详情
    /// </summary>
    /// <param name="id">媒体ID</param>
    /// <returns>媒体信息</returns>
    [HttpGet("{id}")]
    [AllowAnonymous] // 允许匿名访问
    public async Task<IActionResult> GetMedia(ulong id)
    {
        try
        {
            var result = await _mediaService.GetMediaAsync(id);
            return Success(result, "获取媒体详情成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取媒体详情失败，ID: {MediaId}", id);
            return InternalError("获取媒体详情失败");
        }
    }

    /// <summary>
    /// 更新媒体信息
    /// </summary>
    /// <param name="id">媒体ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的媒体信息</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedia(ulong id, [FromBody] UpdateMediaRequest request)
    {
        try
        {
            // 获取当前用户ID
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("无效的用户身份");
            }

            var result = await _mediaService.UpdateMediaAsync(id, request, userId);
            return Success(result, "媒体信息更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体信息更新失败，ID: {MediaId}", id);
            return InternalError("媒体信息更新失败");
        }
    }

    /// <summary>
    /// 获取支持的文件类型
    /// </summary>
    /// <returns>支持的文件类型列表</returns>
    [HttpGet("supported-types")]
    [AllowAnonymous]
    public IActionResult GetSupportedTypes()
    {
        var supportedTypes = new
        {
            images = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" },
            videos = new[] { ".mp4", ".mov", ".avi", ".wmv", ".flv" },
            audios = new[] { ".mp3", ".wav", ".ogg", ".aac" }
        };
        
        return Success(supportedTypes, "获取支持的文件类型成功");
    }

    /// <summary>
    /// 获取文件大小限制
    /// </summary>
    /// <returns>文件大小限制信息</returns>
    [HttpGet("size-limit")]
    [AllowAnonymous]
    public IActionResult GetSizeLimit([FromServices] IConfiguration configuration)
    {
        var maxSize = configuration["Media:MaxFileSize"] ?? "10485760"; // 默认10MB
        var maxSizeMb = long.Parse(maxSize) / (1024 * 1024);
        
        var sizeLimit = new
        {
            maxFileSize = long.Parse(maxSize),
            maxFileSizeMb = maxSizeMb,
            unit = "bytes"
        };
        
        return Success(sizeLimit, "获取文件大小限制成功");
    }
}