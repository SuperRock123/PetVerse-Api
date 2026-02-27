using PetVerse.Core.DTOs.Media;
using PetVerse.Core.Entities;
using PetVerse.Core.Interfaces;

namespace PetVerse.Core.Interfaces;

/// <summary>
/// 媒体服务接口
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// 上传媒体文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="stream">文件流</param>
    /// <param name="postId">关联的帖子ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="displayOrder">显示顺序</param>
    /// <returns>媒体信息</returns>
    Task<MediaResponse> UploadMediaAsync(string fileName, string contentType, Stream stream, ulong postId, ulong userId, ushort displayOrder = 0);

    /// <summary>
    /// 删除媒体文件
    /// </summary>
    /// <param name="mediaId">媒体ID</param>
    /// <param name="userId">用户ID（用于权限验证）</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteMediaAsync(ulong mediaId, ulong userId);

    /// <summary>
    /// 获取帖子的所有媒体文件
    /// </summary>
    /// <param name="postId">帖子ID</param>
    /// <returns>媒体信息集合</returns>
    Task<List<MediaResponse>> GetPostMediasAsync(ulong postId);

    /// <summary>
    /// 获取媒体详情
    /// </summary>
    /// <param name="mediaId">媒体ID</param>
    /// <returns>媒体信息</returns>
    Task<MediaResponse> GetMediaAsync(ulong mediaId);

    /// <summary>
    /// 更新媒体信息
    /// </summary>
    /// <param name="mediaId">媒体ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="userId">用户ID（用于权限验证）</param>
    /// <returns>更新后的媒体信息</returns>
    Task<MediaResponse> UpdateMediaAsync(ulong mediaId, UpdateMediaRequest request, ulong userId);

    /// <summary>
    /// 验证文件类型
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>是否允许上传</returns>
    bool ValidateFileType(string fileName);

    /// <summary>
    /// 验证文件大小
    /// </summary>
    /// <param name="fileSize">文件大小</param>
    /// <returns>是否符合大小限制</returns>
    bool ValidateFileSize(long fileSize);
}