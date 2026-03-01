using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using PetVerse.Core.DTOs;
using PetVerse.Core.DTOs.Media;
using PetVerse.Core.Interfaces;
using PetVerse.Api.Attributes;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using PetVerse.Api.DTOs.Media;

namespace PetVerse.Api.Controllers;

/// <summary>
/// 媒体资源管理控制器
/// </summary>
[ApiController]
[Route("api/media")]
[global::Microsoft.AspNetCore.Authorization.Authorize] // 需要认证，使用全局命名空间避免与自定义 Authorize 特性冲突
public class MediaController : BaseController
{
    private readonly IMediaService _mediaService;
    private readonly IStorageService _storageService;
    private readonly ILogger<MediaController> _logger;
    private readonly IConfiguration _configuration;
    private readonly HashSet<string> _allowedExtensions;

    // 从JWT中解析的当前用户ID
    private ulong _currentUserId => GetCurrentUserId();

    public MediaController(
        IMediaService mediaService,
        IStorageService storageService,
        ILogger<MediaController> logger,
        IConfiguration configuration)
    {
        _mediaService = mediaService;
        _storageService = storageService;
        _logger = logger;
        _configuration = configuration;

        // 从配置读取允许的文件类型
        var allowedExts = configuration["Media:AllowedExtensions"] ?? ".jpg,.jpeg,.png,.gif,.webp,.mp4,.mov,.avi,.wmv,.flv,.mp3,.wav,.ogg,.aac";
        _allowedExtensions = new HashSet<string>(allowedExts.Split(',', StringSplitOptions.RemoveEmptyEntries));
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
                null, // url_path 为 null
                userId);

            // 生成预签名URL
            result.UrlPath = await GeneratePresignedUrl(result.StorageKey);

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

            foreach (var file in request.Files)
            {
                using var stream = file.OpenReadStream();
                var result = await _mediaService.UploadMediaAsync(
                    file.FileName,
                    file.ContentType,
                    stream,
                    null, // url_path 为 null
                    userId);

                // 生成带签名的URL
                result.UrlPath = await GeneratePresignedUrl(result.StorageKey);
                results.Add(result);
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

            // 生成带签名的URL
            result.UrlPath = await GeneratePresignedUrl(result.StorageKey);

            return Success(result, "获取媒体详情成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取媒体详情失败，ID: {MediaId}", id);
            return InternalError("获取媒体详情失败");
        }
    }
    /// <summary>
    /// 获取媒体详情
    /// </summary>
    /// <param name="objectPath">相对路径</param>
    /// <returns>媒体信息</returns>
    [HttpGet]
    public async Task<IActionResult> GetMediaByObjectPath(string objectPath)
    {
        try
        {
            // 解码URL编码的路径
            var decodedPath = Uri.UnescapeDataString(objectPath);

            // 根据相对路径查找媒体记录
            var media = await FindMediaByPathAsync(decodedPath);
            if (media == null)
            {
                return NotFound(new { message = "媒体文件不存在" });
            }

            // 生成带签名的URL（10分钟有效期）
            media.UrlPath = await GeneratePresignedUrl(media.StorageKey);

            return Success(media, "获取媒体详情成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取媒体详情失败，路径: {ObjectPath}", objectPath);
            return InternalError("获取媒体详情失败");
        }
    }

    /// <summary>
    /// 根据相对路径查找媒体记录
    /// </summary>
    /// <param name="objectPath">相对路径</param>
    /// <returns>媒体信息</returns>
    private async Task<MediaResponse?> FindMediaByPathAsync(string objectPath)
    {
        try
        {
            // 移除可能的前导斜杠
            var cleanPath = objectPath.TrimStart('/');

            // 直接通过存储服务检查文件是否存在
            var exists = await _storageService.FileExistsAsync(cleanPath);
            if (!exists)
            {
                return null;
            }

            // 构造基本的媒体响应信息
            var mediaResponse = new MediaResponse
            {
                Id = 0, // 临时ID
                StorageKey = cleanPath,
                UrlPath = "", // 将在调用处生成签名URL
                OriginalName = Path.GetFileName(cleanPath),
                MimeType = GetMimeTypeFromPath(cleanPath),
                MediaType = DetermineMediaTypeFromPath(cleanPath),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return mediaResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据路径查找媒体失败: {ObjectPath}", objectPath);
            return null;
        }
    }

    /// <summary>
    /// 根据文件路径推断MIME类型
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>MIME类型</returns>
    private string GetMimeTypeFromPath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// 根据文件路径推断媒体类型
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>媒体类型</returns>
    private string DetermineMediaTypeFromPath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image", // 图片
            ".mp4" or ".mov" or ".avi" or ".wmv" or ".flv" => "video",   // 视频
            ".mp3" or ".wav" or ".ogg" or ".aac" => "audio",             // 音频
            _ => "unknown" // 未知
        };
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

            // 生成带签名的URL
            result.UrlPath = await GeneratePresignedUrl(result.StorageKey);

            return Success(result, "媒体信息更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "媒体信息更新失败，ID: {MediaId}", id);
            return InternalError("媒体信息更新失败");
        }
    }

    /// <summary>
    /// 获取用户的所有媒体文件
    /// </summary>
    /// <returns>媒体信息集合</returns>
    [HttpGet("user")]
    public async Task<IActionResult> GetUserMedias()
    {
        try
        {
            // 获取当前用户ID
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("无效的用户身份");
            }

            var results = await _mediaService.GetUserMediasAsync(userId);

            // 生成带签名的URLs
            foreach (var result in results)
            {
                result.UrlPath = await GeneratePresignedUrl(result.StorageKey);
            }

            return Success(results, "获取用户媒体文件成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户媒体文件失败");
            return InternalError("获取用户媒体文件失败");
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
    /// 生成预签名URL
    /// </summary>
    /// <param name="request">预签名URL请求参数</param>
    /// <returns>预签名URL信息</returns>
    [HttpPost("presigned-url")]
    [ProducesResponseType(typeof(ApiResponse<PresignedUrlResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GeneratePresignedUrl([FromBody] PresignedUrlRequest request)
    {
        try
        {
            // 验证请求参数
            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                return BadRequest("文件名不能为空");
            }

            if (string.IsNullOrWhiteSpace(request.ContentType))
            {
                return BadRequest("内容类型不能为空");
            }

            // 验证文件类型
            if (!_allowedExtensions.Contains(Path.GetExtension(request.FileName).ToLower()))
            {
                return BadRequest($"不支持的文件类型: {Path.GetExtension(request.FileName)}");
            }

            // 生成存储Key（不实际上传文件，只生成预签名URL）
            var fileExtension = Path.GetExtension(request.FileName);
            var folder = $"temp/{_currentUserId:D}/{DateTime.UtcNow:yyyy/MM/dd}";
            var storageKey = $"{folder}/{Guid.NewGuid():N}{fileExtension}";

            // 生成预签名URL
            var presignedUrl = await GeneratePresignedUrl(storageKey);

            var response = new PresignedUrlResponse
            {
                StorageKey = storageKey,
                PresignedUrl = presignedUrl,
                ExpiresIn = request.ExpireMinutes * 60, // 转换为秒
                Method = "PUT"
            };

            _logger.LogInformation("预签名URL生成成功: StorageKey={StorageKey}, UserId={UserId}",
                storageKey, _currentUserId);

            return Success(response, "预签名URL生成成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成预签名URL失败: UserId={UserId}", _currentUserId);
            return Error("生成预签名URL失败");
        }
    }

    /// <summary>
    /// 获取文件大小限制
    /// </summary>
    /// <returns>文件大小限制信息</returns>
    [HttpGet("size-limit")]
    [AllowAnonymous]
    public IActionResult GetSizeLimit()
    {
        var maxSize = _configuration["Media:MaxFileSize"] ?? "10485760"; // 默认10MB
        var maxSizeMb = long.Parse(maxSize) / (1024 * 1024);

        var sizeLimit = new
        {
            maxFileSize = long.Parse(maxSize),
            maxFileSizeMb = maxSizeMb,
            unit = "bytes"
        };

        return Success(sizeLimit, "获取文件大小限制成功");
    }

    /// <summary>
    /// 生成带签名的URL
    /// </summary>
    /// <param name="storageKey">存储键</param>
    /// <returns>带签名的URL</returns>
    private async Task<string> GeneratePresignedUrl(string storageKey)
    {
        try
        {
            // 使用存储服务生成预签名URL
            // 默认10分钟过期时间
            var presignedUrl = await _storageService.GetFileUrlAsync(storageKey, 10);
            return presignedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成预签名URL失败: {StorageKey}", storageKey);

            // 降级方案：返回公开访问URL
            var endpoint = _configuration["Storage:MinIO:Endpoint"] ?? _configuration["MinIO:Endpoint"] ?? "localhost:9000";
            var bucketName = _configuration["Storage:MinIO:BucketName"] ?? _configuration["MinIO:BucketName"] ?? "petverse";
            var useSsl = bool.TryParse(_configuration["Storage:MinIO:UseSSL"], out var ssl) && ssl;

            var protocol = useSsl ? "https" : "http";
            return $"{protocol}://{endpoint}/{bucketName}/{storageKey}";
        }
    }
}