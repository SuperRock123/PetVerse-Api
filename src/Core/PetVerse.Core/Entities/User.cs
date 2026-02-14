using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class User
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("username")]
    [StringLength(64)]
    public string Username { get; set; } = string.Empty;
    
    [Column("nickname")]
    [StringLength(64)]
    public string? Nickname { get; set; }
    
    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [Column("email")]
    [StringLength(128)]
    public string? Email { get; set; }
    
    [Column("password_hash")]
    [StringLength(255)]
    public string? PasswordHash { get; set; }
    
    [Column("avatar_url")]
    [StringLength(512)]
    public string? AvatarUrl { get; set; }
    
    [Column("bio")]
    public string? Bio { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }
    
    [Column("status")]
    public sbyte Status { get; set; } = 1; // 1=正常 0=禁用 -1=注销
    
    // 导航属性
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<UserTagRelation> UserTagRelations { get; set; } = new List<UserTagRelation>();
}