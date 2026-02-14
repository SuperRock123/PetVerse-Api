using System.ComponentModel.DataAnnotations;

namespace PetVerse.Core.DTOs.User;

/// <summary>
/// 用户创建请求DTO
/// </summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(64, ErrorMessage = "用户名长度不能超过64个字符")]
    public string Username { get; set; } = string.Empty;

    [StringLength(64, ErrorMessage = "昵称长度不能超过64个字符")]
    public string? Nickname { get; set; }

    [Phone(ErrorMessage = "手机号格式不正确")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(128, ErrorMessage = "邮箱长度不能超过128个字符")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
    public string Password { get; set; } = string.Empty;

    [StringLength(512, ErrorMessage = "头像URL长度不能超过512个字符")]
    public string? AvatarUrl { get; set; }

    [StringLength(500, ErrorMessage = "个人简介长度不能超过500个字符")]
    public string? Bio { get; set; }
}

/// <summary>
/// 用户更新请求DTO
/// </summary>
public class UpdateUserRequest
{
    [StringLength(64, ErrorMessage = "昵称长度不能超过64个字符")]
    public string? Nickname { get; set; }

    [Phone(ErrorMessage = "手机号格式不正确")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(128, ErrorMessage = "邮箱长度不能超过128个字符")]
    public string? Email { get; set; }

    [StringLength(512, ErrorMessage = "头像URL长度不能超过512个字符")]
    public string? AvatarUrl { get; set; }

    [StringLength(500, ErrorMessage = "个人简介长度不能超过500个字符")]
    public string? Bio { get; set; }
}

/// <summary>
/// 用户登录请求DTO
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "用户名或邮箱不能为空")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 用户响应DTO
/// </summary>
public class UserResponse
{
    public ulong Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public sbyte Status { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// 用户详细响应DTO（包含关联数据）
/// </summary>
public class UserDetailResponse : UserResponse
{
    public int PetsCount { get; set; }
    public int PostsCount { get; set; }
    public new List<string> Tags { get; set; } = new();
}

/// <summary>
/// 登录响应DTO
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = new();
    public string Role { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// 用户查询参数DTO
/// </summary>
public class UserQueryParams : PaginationInfo
{
    public string? Keyword { get; set; }
    public sbyte? Status { get; set; }
}