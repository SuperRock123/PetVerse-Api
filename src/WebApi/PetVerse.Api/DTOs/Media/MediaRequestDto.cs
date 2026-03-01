using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetVerse.Api.DTOs.Media;

/// <summary>
/// 媒体上传请求DTO
/// </summary>
public class UploadMediaRequest
{
    [Required(ErrorMessage = "必须选择文件")]
    public IFormFile File { get; set; } = null!;
}

/// <summary>
/// 批量上传媒体请求DTO
/// </summary>
public class UploadMediasRequest
{
    [Required(ErrorMessage = "必须选择文件")]
    public List<IFormFile> Files { get; set; } = new();
}

/// <summary>
/// 预签名URL请求DTO
/// </summary>
public class PresignedUrlRequest
{
    /// <summary>
    /// 文件名
    /// </summary>
    [Required(ErrorMessage = "文件名不能为空")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 内容类型
    /// </summary>
    [Required(ErrorMessage = "内容类型不能为空")]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（分钟），默认10分钟
    /// </summary>
    public int ExpireMinutes { get; set; } = 10;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long? FileSize { get; set; }
}

/// <summary>
/// 预签名URL响应DTO
/// </summary>
public class PresignedUrlResponse
{
    /// <summary>
    /// 存储键
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>
    /// 预签名URL
    /// </summary>
    public string PresignedUrl { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（秒）
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string Method { get; set; } = "PUT";

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 内容类型
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}