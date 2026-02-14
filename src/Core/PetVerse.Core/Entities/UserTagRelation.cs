using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class UserTagRelation
{
    [Column("user_id")]
    public ulong UserId { get; set; }
    
    [Column("tag_id")]
    public ulong TagId { get; set; }
    
    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    [Column("assigned_by")]
    public ulong? AssignedBy { get; set; }
    
    // 导航属性
    public virtual User User { get; set; } = null!;
    public virtual UserTag UserTag { get; set; } = null!;
}