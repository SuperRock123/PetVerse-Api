using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetVerse.Core.Entities;

public class Report
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }
    
    [Column("reporter_id")]
    public ulong ReporterId { get; set; }
    
    [Column("target_type")]
    [StringLength(32)]
    public string TargetType { get; set; } = string.Empty; // post / comment / user
    
    [Column("target_id")]
    public ulong TargetId { get; set; }
    
    [Column("reason_type")]
    public sbyte ReasonType { get; set; } // 1=色情 2=暴力 3=广告 ...
    
    [Column("reason_detail")]
    public string? ReasonDetail { get; set; }
    
    [Column("status")]
    public sbyte Status { get; set; } = 0; // 0=待处理 1=已处理 2=忽略 -1=无效
    
    [Column("handled_by")]
    public ulong? HandledBy { get; set; }
    
    [Column("handled_at")]
    public DateTime? HandledAt { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 导航属性
    public virtual User Reporter { get; set; } = null!;
    public virtual User? Handler { get; set; }
}