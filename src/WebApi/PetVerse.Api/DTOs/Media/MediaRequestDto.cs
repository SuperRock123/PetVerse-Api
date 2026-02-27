using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetVerse.Api.DTOs.Media;

/// <summary>
/// 媒体上传请求DTO
/// </summary>
public class UploadMediaRequest
{
    [Required(ErrorMessage = "必须选择文件")]
    public IFormFile File { get; set; } = null!;

    [Required(ErrorMessage = "帖子ID不能为空")]
    public ulong PostId { get; set; }

    public ushort DisplayOrder { get; set; } = 0;
}

/// <summary>
/// 批量上传媒体请求DTO
/// </summary>
public class UploadMediasRequest
{
    [Required(ErrorMessage = "必须选择文件")]
    public List<IFormFile> Files { get; set; } = new();

    [Required(ErrorMessage = "帖子ID不能为空")]
    public ulong PostId { get; set; }
}