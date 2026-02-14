using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class Like
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("target_type")]
    [StringLength(32)]
    public string TargetType { get; set; } = string.Empty; // post / comment
    
    [Column("target_id")]
    public ulong TargetId { get; set; }
    
    [Column("user_id")]
    public ulong UserId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual User User { get; set; } = null!;
    // 注意：由于多态关系，这里不能直接导航到Post或Comment
}