using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

/// <summary>
/// 统一媒体资源实体
/// </summary>
public class MediaResource
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("user_id")]
    public ulong UserId { get; set; }
    
    [Column("media_type")]
    public int MediaTypeInt { get; set; } = 0; // 0=image, 1=video, 2=audio, 3=other
    
    [NotMapped]
    public string MediaType
    {
        get
        {
            switch (MediaTypeInt)
            {
                case 0: return "image";
                case 1: return "video";
                case 2: return "audio";
                case 3: return "other";
                default: return "other";
            }
        }
        set
        {
            switch (value.ToLower())
            {
                case "image": MediaTypeInt = 0;
                    break;
                case "video": MediaTypeInt = 1;
                    break;
                case "audio": MediaTypeInt = 2;
                    break;
                case "other": MediaTypeInt = 3;
                    break;
                default: MediaTypeInt = 3;
                    break;
            }
        }
    }
    
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
    
    [Column("status")]
    public sbyte Status { get; set; } = 1; // 1=正常 0=删除 -1=处理中
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual User User { get; set; } = null!;
}