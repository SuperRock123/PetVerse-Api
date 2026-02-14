using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class UserTag
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("tag_name")]
    [StringLength(32)]
    public string TagName { get; set; } = string.Empty;
    
    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }
    
    [Column("color")]
    [StringLength(16)]
    public string? Color { get; set; }
    
    [Column("icon_url")]
    [StringLength(512)]
    public string? IconUrl { get; set; }
    
    [Column("extra_info")]
    public string? ExtraInfo { get; set; } // JSON字段
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual ICollection<UserTagRelation> UserTagRelations { get; set; } = new List<UserTagRelation>();
}