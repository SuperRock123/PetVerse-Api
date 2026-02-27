using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace PetVerse.Core.Entities;

/// <summary>
/// 帖子媒体文件实体
/// </summary>
public class PostMedia
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("post_id")]
    public ulong PostId { get; set; }
    
    [Column("media_type")]
    public MediaType MediaType { get; set; }
    
    [Column("mime_type")]
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;
    
    [Column("original_name")]
    [StringLength(255)]
    public string? OriginalName { get; set; }
    
    [Column("storage_key")]
    [StringLength(512)]
    public string StorageKey { get; set; } = string.Empty;
    
    [Column("url_path")]
    [StringLength(512)]
    public string? UrlPath { get; set; }
    
    [Column("meta")]
    public string? Meta { get; set; } // JSON字符串
    
    [Column("display_order")]
    public ushort DisplayOrder { get; set; } = 0;
    
    [Column("status")]
    public sbyte Status { get; set; } = 1; // 1=正常 0=删除 -1=处理中
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual Post Post { get; set; } = null!;
}

/// <summary>
/// 媒体类型枚举
/// </summary>
public enum MediaType
{
    Image = 1,
    Video = 2,
    Audio = 3,
    Other = 4
}