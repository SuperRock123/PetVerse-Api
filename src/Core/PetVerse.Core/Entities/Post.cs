using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class Post
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("user_id")]
    public ulong UserId { get; set; }
    
    [Column("pet_id")]
    public ulong? PetId { get; set; }
    
    [Column("content")]
    public string Content { get; set; } = string.Empty;
    
    [Column("location")]
    [StringLength(128)]
    public string? Location { get; set; }
    
    // 统计字段
    [Column("likes_count")]
    public uint LikesCount { get; set; } = 0;
    
    [Column("comments_count")]
    public uint CommentsCount { get; set; } = 0;
    
    [Column("view_count")]
    public uint ViewCount { get; set; } = 0;
    
    [Column("media_count")]
    public byte? MediaCount { get; set; }

    [Column("media_ids")]
    public string? MediaIds { get; set; }

    [Column("visibility")]
    public sbyte Visibility { get; set; } = 1; // 1=公开 2=仅好友 3=私密
    
    [Column("status")]
    public sbyte Status { get; set; } = 1; // 1=正常 0=已删除 -1=审核中 -2=已屏蔽
    
    [Column("published_at")]
    public DateTime? PublishedAt { get; set; } // 发布时间（支持延迟发布）
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual User User { get; set; } = null!;
    public virtual Pet? Pet { get; set; }
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
}