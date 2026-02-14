using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class Pettag
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("serial_number")]
    [StringLength(64)]
    public string SerialNumber { get; set; } = string.Empty;
    
    [Column("user_id")]
    public ulong? UserId { get; set; }
    
    [Column("pet_id")]
    public ulong? PetId { get; set; }
    
    [Column("status")]
    public sbyte Status { get; set; } = 0; // 0=未激活 1=在线 2=离线
    
    [Column("battery_level")]
    public sbyte? BatteryLevel { get; set; }
    
    [Column("last_seen")]
    public DateTime? LastSeen { get; set; }
    
    [Column("firmware_version")]
    [StringLength(32)]
    public string? FirmwareVersion { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual User? User { get; set; }
    public virtual Pet? Pet { get; set; }
}