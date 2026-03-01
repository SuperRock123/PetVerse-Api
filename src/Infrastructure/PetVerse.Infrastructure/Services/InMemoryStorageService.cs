using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetVerse.Core.Interfaces;
using System.IO;

namespace PetVerse.Infrastructure.Services;

/// <summary>
/// 内存存储服务实现（用于开发和测试）
/// </summary>
public class InMemoryStorageService : IStorageService
{
    private readonly ILogger<InMemoryStorageService> _logger;
    private readonly Dictionary<string, byte[]> _storage = new();

    public InMemoryStorageService(IConfiguration configuration, ILogger<InMemoryStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<(string Url, string Key)> UploadFileAsync(string fileName, string contentType, Stream stream, string folder)
    {
        try
        {
            // 生成存储Key
            var fileExtension = Path.GetExtension(fileName);
            var key = $"{folder}/{Guid.NewGuid():N}{fileExtension}";

            // 读取文件内容到内存
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            _storage[key] = memoryStream.ToArray();

            // 生成访问URL（模拟）
            var url = $"http://localhost:8080/storage/{key}";

            _logger.LogInformation("文件上传成功: {Key}, 大小: {Size} bytes", key, memoryStream.Length);
            
            return (url, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败: {FileName}", fileName);
            throw new InvalidOperationException($"文件上传失败: {ex.Message}");
        }
    }

    public async Task<bool> DeleteFileAsync(string key)
    {
        try
        {
            var result = _storage.Remove(key);
            if (result)
            {
                _logger.LogInformation("文件删除成功: {Key}", key);
            }
            else
            {
                _logger.LogWarning("文件不存在: {Key}", key);
            }
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件删除失败: {Key}", key);
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string key, int expireMinutes = 0)
    {
        try
        {
            if (_storage.ContainsKey(key))
            {
                return $"http://localhost:8080/storage/{key}";
            }
            throw new FileNotFoundException($"文件不存在: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件URL失败: {Key}", key);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string key)
    {
        return await Task.FromResult(_storage.ContainsKey(key));
    }
}