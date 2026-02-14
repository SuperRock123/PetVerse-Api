using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class Comment
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("post_id")]
    public ulong PostId { get; set; }
    
    [Column("user_id")]
    public ulong UserId { get; set; }
    
    [Column("parent_id")]
    public ulong? ParentId { get; set; }
    
    [Column("content")]
    public string Content { get; set; } = string.Empty;
    
    [Column("likes_count")]
    public uint LikesCount { get; set; } = 0;
    
    [Column("status")]
    public sbyte Status { get; set; } = 1;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual Post Post { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Comment? Parent { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
}