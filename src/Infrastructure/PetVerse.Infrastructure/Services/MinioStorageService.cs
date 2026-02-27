using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using PetVerse.Core.Interfaces;

namespace PetVerse.Infrastructure.Services;

/// <summary>
/// MinIO存储服务实现
/// </summary>
public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioStorageService> logger)
    {
        _minioClient = minioClient;
        _bucketName = configuration["Storage:MinIO:BucketName"] ?? "petverse";
        _logger = logger;
    }

    public async Task<(string Url, string Key)> UploadFileAsync(string fileName, string contentType, Stream stream, string folder)
    {
        try
        {
            // 生成存储Key
            var fileExtension = Path.GetExtension(fileName);
            var key = $"{folder}/{Guid.NewGuid():N}{fileExtension}";

            // 确保存储桶存在
            await EnsureBucketExistsAsync();

            // 上传文件
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(key)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            // 获取访问URL
            var url = await GetFileUrlAsync(key);

            _logger.LogInformation("文件上传成功: {Key}, 大小: {Size} bytes", key, stream.Length);
            
            return (url, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败: {FileName}", fileName);
            throw new InvalidOperationException($"文件上传失败: {ex.Message}");
        }
    }

    // 批量上传功能暂不实现
    // public async Task<List<(string Url, string Key)>> UploadFilesAsync(IEnumerable<IFormFile> files, string folder)
    // {
    //     var results = new List<(string Url, string Key)>();
    //     
    //     foreach (var file in files)
    //     {
    //         var result = await UploadFileAsync(file, folder);
    //         results.Add(result);
    //     }
    //     
    //     return results;
    // }

    public async Task<bool> DeleteFileAsync(string key)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(key);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
            
            _logger.LogInformation("文件删除成功: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件删除失败: {Key}", key);
            return false;
        }
    }

    public async Task<int> DeleteFilesAsync(IEnumerable<string> keys)
    {
        var successCount = 0;
        
        foreach (var key in keys)
        {
            if (await DeleteFileAsync(key))
            {
                successCount++;
            }
        }
        
        return successCount;
    }

    public async Task<string> GetFileUrlAsync(string key, int expireMinutes = 0)
    {
        try
        {
            if (expireMinutes > 0)
            {
                // 生成临时签名URL
                var presignArgs = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(key)
                    .WithExpiry(expireMinutes * 60);

                return await _minioClient.PresignedGetObjectAsync(presignArgs);
            }
            else
            {
                // 返回公共访问URL
                return $"http://localhost:9000/{_bucketName}/{key}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件URL失败: {Key}", key);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string key)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(key);

            await _minioClient.StatObjectAsync(statObjectArgs);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string key)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(key);

            var result = await _minioClient.StatObjectAsync(statObjectArgs);
            
            return new FileMetadata
            {
                Key = key,
                ContentType = result.ContentType,
                Size = result.Size,
                LastModified = result.LastModified,
                CustomMetadata = new Dictionary<string, string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件元数据失败: {Key}", key);
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
            var exists = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!exists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                _logger.LogInformation("存储桶创建成功: {BucketName}", _bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查或创建存储桶失败: {BucketName}", _bucketName);
            throw;
        }
    }
}