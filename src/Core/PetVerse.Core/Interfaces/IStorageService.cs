namespace PetVerse.Core.Interfaces;

/// <summary>
/// 存储服务接口
/// 支持多种存储后端（MinIO、阿里云OSS、腾讯云COS等）
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="stream">文件流</param>
    /// <param name="folder">存储文件夹</param>
    /// <returns>文件访问URL和存储Key</returns>
    Task<(string Url, string Key)> UploadFileAsync(string fileName, string contentType, Stream stream, string folder);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="key">存储Key</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteFileAsync(string key);

    /// <summary>
    /// 获取文件访问URL
    /// </summary>
    /// <param name="key">存储Key</param>
    /// <param name="expireMinutes">过期时间（分钟），默认不过期</param>
    /// <returns>文件访问URL</returns>
    Task<string> GetFileUrlAsync(string key, int expireMinutes = 0);

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="key">存储Key</param>
    /// <returns>文件是否存在</returns>
    Task<bool> FileExistsAsync(string key);
}

/// <summary>
/// 文件元数据
/// </summary>
public class FileMetadata
{
    public string Key { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public Dictionary<string, string> CustomMetadata { get; set; } = new();
}