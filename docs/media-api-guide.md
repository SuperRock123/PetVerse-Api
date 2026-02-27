# PetVerse 媒体资源管理接口文档

## 概述

PetVerse 提供了完整的媒体资源管理功能，支持图片、视频、音频等多种媒体类型的上传、存储和管理。系统采用灵活的存储抽象层设计，当前使用 MinIO 作为存储后端，未来可轻松迁移到阿里云 OSS、腾讯云 COS 等云存储服务。

## 功能特性

- ✅ 支持多种媒体类型（图片、视频、音频）
- ✅ 文件类型和大小验证
- ✅ 批量上传和删除
- ✅ 媒体元数据管理
- ✅ 权限控制（仅允许上传者操作自己的媒体）
- ✅ 灵活的存储后端抽象（支持MinIO、阿里云OSS、腾讯云COS等）

## 技术架构

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   MediaController│────│   IMediaService  │────│  IStorageService│
└─────────────────┘    └──────────────────┘    └─────────────────┘
                              │                         │
                              ▼                         ▼
                    ┌──────────────────┐    ┌─────────────────┐
                    │   MediaService   │    │MinioStorageService│
                    └──────────────────┘    └─────────────────┘
```

## API 接口列表

### 1. 上传单个媒体文件
```
POST /api/media
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form Fields:
- file: 文件数据
- postId: 关联的帖子ID
- displayOrder: 显示顺序（可选，默认0）
```

### 2. 批量上传媒体文件
```
POST /api/media/batch
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form Fields:
- files: 文件数组
- postId: 关联的帖子ID
```

### 3. 删除媒体文件
```
DELETE /api/media/{id}
Authorization: Bearer {token}
```

### 4. 批量删除媒体文件
```
DELETE /api/media/batch
Content-Type: application/json
Authorization: Bearer {token}

Body: [1, 2, 3] // 媒体ID数组
```

### 5. 获取帖子媒体文件
```
GET /api/media/post/{postId}
```

### 6. 获取媒体详情
```
GET /api/media/{id}
```

### 7. 更新媒体信息
```
PUT /api/media/{id}
Content-Type: application/json
Authorization: Bearer {token}

Body: {
  "originalName": "new-name.jpg",
  "displayOrder": 1
}
```

### 8. 获取支持的文件类型
```
GET /api/media/supported-types
```

### 9. 获取文件大小限制
```
GET /api/media/size-limit
```

## 配置说明

### appsettings.json 配置

```json
{
  "Media": {
    "AllowedExtensions": ".jpg,.jpeg,.png,.gif,.webp,.mp4,.mov,.avi,.wmv,.flv,.mp3,.wav,.ogg,.aac",
    "MaxFileSize": 10485760,
    "MaxBatchUpload": 10
  },
  "Storage": {
    "Provider": "MinIO",
    "MinIO": {
      "Endpoint": "localhost:9000",
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin",
      "BucketName": "petverse",
      "UseSSL": false
    }
  }
}
```

## 部署指南

### 1. 本地开发环境（MinIO）

1. 启动 MinIO 服务：
```bash
docker run -p 9000:9000 -p 9001:9001 \
  -e "MINIO_ROOT_USER=minioadmin" \
  -e "MINIO_ROOT_PASSWORD=minioadmin" \
  -v /tmp/minio-data:/data \
  quay.io/minio/minio server /data --console-address ":9001"
```

2. 访问 MinIO 控制台：http://localhost:9001
3. 使用默认账号密码登录：minioadmin/minioadmin
4. 创建存储桶：petverse

### 2. 生产环境部署

#### 阿里云 OSS 配置示例：
```json
{
  "Storage": {
    "Provider": "AliyunOSS",
    "AliyunOSS": {
      "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
      "AccessKeyId": "your-access-key-id",
      "AccessKeySecret": "your-access-key-secret",
      "BucketName": "your-bucket-name"
    }
  }
}
```

#### 腾讯云 COS 配置示例：
```json
{
  "Storage": {
    "Provider": "TencentCOS",
    "TencentCOS": {
      "AppId": "your-app-id",
      "Region": "ap-beijing",
      "SecretId": "your-secret-id",
      "SecretKey": "your-secret-key",
      "BucketName": "your-bucket-name"
    }
  }
}
```

## 测试使用

### 使用测试脚本
```bash
# Windows
test-media.bat

# Linux/Mac
chmod +x test-media.sh
./test-media.sh
```

### 手动测试示例

1. 获取认证令牌：
```bash
curl -X POST "http://localhost:5124/api/user/login" \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"testuser","password":"password123"}'
```

2. 上传媒体文件：
```bash
curl -X POST "http://localhost:5124/api/media" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "file=@test-image.jpg" \
  -F "postId=1"
```

## 扩展开发

### 添加新的存储提供商

1. 实现 `IStorageService` 接口
2. 在 `Program.cs` 中注册服务
3. 更新配置文件

```csharp
public class AliyunOssStorageService : IStorageService
{
    // 实现所有接口方法
}
```

### 自定义文件验证规则

在 `MediaService` 中修改验证逻辑：
```csharp
public bool ValidateFileType(IFormFile file)
{
    // 自定义验证规则
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    return _allowedExtensions.Contains(extension) && CustomValidation(file);
}
```

## 注意事项

1. **安全性**：所有媒体操作都需要有效的 JWT 认证
2. **权限控制**：用户只能操作自己上传的媒体文件
3. **文件清理**：删除媒体记录时会同时删除存储中的文件
4. **性能优化**：建议对大文件上传实现分片上传
5. **CDN集成**：生产环境建议配置 CDN 加速访问

## 故障排除

### 常见问题

1. **上传失败**：检查 MinIO 服务是否正常运行
2. **权限错误**：确认 JWT token 有效且用户有操作权限
3. **文件类型限制**：检查配置文件中的允许扩展名
4. **存储空间不足**：监控存储使用情况，及时扩容

### 日志查看
```bash
# 查看应用日志
tail -f logs/petverse-api.log

# 查看 MinIO 日志
docker logs minio-container
```

## 后续规划

- [ ] 支持图片压缩和格式转换
- [ ] 实现视频转码功能
- [ ] 添加水印功能
- [ ] 支持 CDN 自动刷新
- [ ] 实现智能裁剪和缩略图生成
- [ ] 添加媒体内容审核功能