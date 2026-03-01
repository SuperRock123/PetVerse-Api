using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetVerse.Api.DTOs.Post;

/// <summary>
/// 上传完整帖子请求DTO
/// </summary>
public class UploadPostRequest
{
    [Required(ErrorMessage = "用户ID不能为空")]
    public ulong UserId { get; set; }

    public ulong? PetId { get; set; }

    [Required(ErrorMessage = "内容不能为空")]
    [StringLength(2000, ErrorMessage = "内容长度不能超过2000个字符")]
    public string Content { get; set; } = string.Empty;

    [StringLength(128, ErrorMessage = "位置信息长度不能超过128个字符")]
    public string? Location { get; set; }

    [Range(1, 3, ErrorMessage = "可见性必须在1-3之间")]
    public sbyte Visibility { get; set; } = 1; // 1=公开 2=仅好友 3=私密

    public List<IFormFile>? Files { get; set; }
}
