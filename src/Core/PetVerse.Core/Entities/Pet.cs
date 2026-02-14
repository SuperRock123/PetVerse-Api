using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class Pet
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("user_id")]
    public ulong UserId { get; set; }
    
    [Column("name")]
    [StringLength(64)]
    public string Name { get; set; } = string.Empty;
    
    [Column("breed")]
    [StringLength(128)]
    public string? Breed { get; set; }
    
    [Column("gender")]
    public sbyte? Gender { get; set; } // 0=未知 1=公 2=母
    
    [Column("birthday")]
    public DateOnly? Birthday { get; set; }
    
    [Column("weight_kg")]
    public decimal? WeightKg { get; set; }
    
    [Column("health_status")]
    [StringLength(64)]
    public string? HealthStatus { get; set; }
    
    [Column("avatar_url")]
    [StringLength(512)]
    public string? AvatarUrl { get; set; }
    
    [Column("pettag_id")]
    [StringLength(64)]
    public string? PettagId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<PetVaccine> PetVaccines { get; set; } = new List<PetVaccine>();
}