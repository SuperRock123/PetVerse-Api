using System.IO;
using PetVerse.Core.DTOs.Media;

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
    /// <param name="urlPath">URL路径（可选）</param>
    /// <param name="userId">用户ID</param>
    /// <returns>媒体信息</returns>
    Task<MediaResponse> UploadMediaAsync(string fileName, string contentType, Stream stream, string? urlPath, ulong userId);

    /// <summary>
    /// 删除媒体文件
    /// </summary>
    /// <param name="mediaId">媒体ID</param>
    /// <param name="userId">用户ID（用于权限验证）</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteMediaAsync(ulong mediaId, ulong userId);

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
    /// 获取用户的所有媒体文件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>媒体信息集合</returns>
    Task<List<MediaResponse>> GetUserMediasAsync(ulong userId);

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

    /// <summary>
    /// 通过存储键删除媒体文件
    /// </summary>
    /// <param name="storageKey">存储键</param>
    /// <param name="userId">用户ID（用于权限验证）</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteMediaByStorageKeyAsync(string storageKey, ulong userId);

    /// <summary>
    /// 通过存储键批量删除媒体文件
    /// </summary>
    /// <param name="storageKeys">存储键集合</param>
    /// <param name="userId">用户ID（用于权限验证）</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteMediasByStorageKeysAsync(List<string> storageKeys, ulong userId);
}