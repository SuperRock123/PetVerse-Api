using System.ComponentModel.DataAnnotations;

namespace PetVerse.Core.DTOs.Media;

/// <summary>
/// 媒体响应DTO
/// </summary>
public class MediaResponse
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string? OriginalName { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string? UrlPath { get; set; }
    public object? Meta { get; set; }
    public sbyte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 更新媒体请求DTO
/// </summary>
public class UpdateMediaRequest
{
    [StringLength(255, ErrorMessage = "原始文件名长度不能超过255个字符")]
    public string? OriginalName { get; set; }
    
    public object? Meta { get; set; }
    
    public sbyte? Status { get; set; }
}

/// <summary>
/// 媒体查询参数DTO
/// </summary>
public class MediaQueryParams
{
    public ulong? UserId { get; set; }
    public string? MediaType { get; set; }
    public sbyte? Status { get; set; }
}