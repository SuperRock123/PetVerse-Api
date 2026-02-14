using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class SystemConfig
{
    [Key]
    [Column("key")]
    [StringLength(128)]
    public string Key { get; set; } = string.Empty;
    
    [Column("value")]
    public string Value { get; set; } = string.Empty;
    
    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}