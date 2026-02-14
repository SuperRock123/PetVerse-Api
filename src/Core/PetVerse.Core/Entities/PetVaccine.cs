using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class PetVaccine
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("pet_id")]
    public ulong PetId { get; set; }
    
    [Column("vaccine_name")]
    [StringLength(128)]
    public string VaccineName { get; set; } = string.Empty;
    
    [Column("vaccinate_date")]
    public DateOnly VaccinateDate { get; set; }
    
    [Column("next_date")]
    public DateOnly? NextDate { get; set; }
    
    [Column("hospital")]
    [StringLength(128)]
    public string? Hospital { get; set; }
    
    [Column("remark")]
    public string? Remark { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual Pet Pet { get; set; } = null!;
}