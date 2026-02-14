using System.ComponentModel.DataAnnotations;

namespace PetVerse.Core.DTOs.Post;

/// <summary>
/// 帖子创建请求DTO
/// </summary>
public class CreatePostRequest
{
    [Required(ErrorMessage = "用户ID不能为空")]
    public ulong UserId { get; set; }

    public ulong? PetId { get; set; }

    [Required(ErrorMessage = "内容不能为空")]
    [StringLength(2000, ErrorMessage = "内容长度不能超过2000个字符")]
    public string Content { get; set; } = string.Empty;

    public List<string>? MediaUrls { get; set; }

    [StringLength(128, ErrorMessage = "位置信息长度不能超过128个字符")]
    public string? Location { get; set; }

    [Range(1, 3, ErrorMessage = "可见性必须在1-3之间")]
    public sbyte Visibility { get; set; } = 1; // 1=公开 2=仅好友 3=私密
}

/// <summary>
/// 帖子更新请求DTO
/// </summary>
public class UpdatePostRequest
{
    [StringLength(2000, ErrorMessage = "内容长度不能超过2000个字符")]
    public string? Content { get; set; }

    public List<string>? MediaUrls { get; set; }

    [StringLength(128, ErrorMessage = "位置信息长度不能超过128个字符")]
    public string? Location { get; set; }

    [Range(1, 3, ErrorMessage = "可见性必须在1-3之间")]
    public sbyte? Visibility { get; set; }
}

/// <summary>
/// 帖子响应DTO
/// </summary>
public class PostResponse
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong? PetId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> MediaUrls { get; set; } = new();
    public string? Location { get; set; }
    public uint LikesCount { get; set; }
    public uint CommentsCount { get; set; }
    public sbyte Visibility { get; set; }
    public sbyte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // 关联用户信息
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string? Nickname { get; set; }
    
    // 关联宠物信息
    public string? PetName { get; set; }
    public string? PetAvatar { get; set; }
}

/// <summary>
/// 帖子详细响应DTO
/// </summary>
public class PostDetailResponse : PostResponse
{
    public List<CommentInfo> Comments { get; set; } = new();
    public bool IsLiked { get; set; } // 当前用户是否点赞
}

/// <summary>
/// 评论信息DTO
/// </summary>
public class CommentInfo
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public uint LikesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // 用户信息
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string? Nickname { get; set; }
}

/// <summary>
/// 帖子查询参数DTO
/// </summary>
public class PostQueryParams : PaginationInfo
{
    public ulong? UserId { get; set; }
    public ulong? PetId { get; set; }
    public string? Keyword { get; set; }
    public sbyte? Visibility { get; set; }
    public sbyte? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// 评论创建请求DTO
/// </summary>
public class CreateCommentRequest
{
    [Required(ErrorMessage = "用户ID不能为空")]
    public ulong UserId { get; set; }

    [Required(ErrorMessage = "帖子ID不能为空")]
    public ulong PostId { get; set; }

    public ulong? ParentId { get; set; }

    [Required(ErrorMessage = "评论内容不能为空")]
    [StringLength(1000, ErrorMessage = "评论内容长度不能超过1000个字符")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 点赞请求DTO
/// </summary>
public class LikeRequest
{
    [Required(ErrorMessage = "用户ID不能为空")]
    public ulong UserId { get; set; }

    [Required(ErrorMessage = "目标类型不能为空")]
    public string TargetType { get; set; } = string.Empty; // "post" 或 "comment"

    [Required(ErrorMessage = "目标ID不能为空")]
    public ulong TargetId { get; set; }
}