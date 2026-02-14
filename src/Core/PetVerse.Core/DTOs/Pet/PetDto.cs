using System.ComponentModel.DataAnnotations;

namespace PetVerse.Core.DTOs.Pet;

/// <summary>
/// 宠物创建请求DTO
/// </summary>
public class CreatePetRequest
{
    [Required(ErrorMessage = "用户ID不能为空")]
    public ulong UserId { get; set; }

    [Required(ErrorMessage = "宠物名称不能为空")]
    [StringLength(64, ErrorMessage = "宠物名称长度不能超过64个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(128, ErrorMessage = "品种长度不能超过128个字符")]
    public string? Breed { get; set; }

    public sbyte? Gender { get; set; } // 0=未知 1=公 2=母

    public DateOnly? Birthday { get; set; }

    [Range(0.1, 100, ErrorMessage = "体重必须在0.1-100kg之间")]
    public decimal? WeightKg { get; set; }

    [StringLength(64, ErrorMessage = "健康状况长度不能超过64个字符")]
    public string? HealthStatus { get; set; }

    [StringLength(512, ErrorMessage = "头像URL长度不能超过512个字符")]
    public string? AvatarUrl { get; set; }

    [StringLength(64, ErrorMessage = "宠物标签ID长度不能超过64个字符")]
    public string? PettagId { get; set; }
}

/// <summary>
/// 宠物更新请求DTO
/// </summary>
public class UpdatePetRequest
{
    [StringLength(64, ErrorMessage = "宠物名称长度不能超过64个字符")]
    public string? Name { get; set; }

    [StringLength(128, ErrorMessage = "品种长度不能超过128个字符")]
    public string? Breed { get; set; }

    public sbyte? Gender { get; set; }

    public DateOnly? Birthday { get; set; }

    [Range(0.1, 100, ErrorMessage = "体重必须在0.1-100kg之间")]
    public decimal? WeightKg { get; set; }

    [StringLength(64, ErrorMessage = "健康状况长度不能超过64个字符")]
    public string? HealthStatus { get; set; }

    [StringLength(512, ErrorMessage = "头像URL长度不能超过512个字符")]
    public string? AvatarUrl { get; set; }

    [StringLength(64, ErrorMessage = "宠物标签ID长度不能超过64个字符")]
    public string? PettagId { get; set; }
}

/// <summary>
/// 宠物响应DTO
/// </summary>
public class PetResponse
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public sbyte? Gender { get; set; }
    public DateOnly? Birthday { get; set; }
    public decimal? WeightKg { get; set; }
    public string? HealthStatus { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PettagId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // 关联用户信息
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
}

/// <summary>
/// 宠物详细响应DTO
/// </summary>
public class PetDetailResponse : PetResponse
{
    public int PostsCount { get; set; }
    public int VaccinesCount { get; set; }
    public List<VaccineInfo> Vaccines { get; set; } = new();
}

/// <summary>
/// 疫苗信息DTO
/// </summary>
public class VaccineInfo
{
    public string VaccineName { get; set; } = string.Empty;
    public DateOnly VaccinationDate { get; set; }
    public DateOnly? NextDueDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// 宠物查询参数DTO
/// </summary>
public class PetQueryParams : PaginationInfo
{
    public ulong? UserId { get; set; }
    public string? Keyword { get; set; }
    public string? Breed { get; set; }
    public sbyte? Gender { get; set; }
}