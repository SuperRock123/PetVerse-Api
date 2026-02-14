# PetVerse API

宠物社交平台后端API服务

## 技术栈

- **框架**: .NET 8
- **架构**: Clean Architecture
- **数据库**: MySQL 8.0+ / PostgreSQL (TimescaleDB)
- **ORM**: Entity Framework Core 8+
- **缓存**: Redis 7+
- **消息队列**: RabbitMQ
- **对象存储**: MinIO (S3兼容)
- **认证**: JWT Bearer
- **日志**: Serilog
- **API文档**: Swagger/OpenAPI
- **容器化**: Docker + Docker Compose

## 项目结构

```
PetVerse-Api/
├── src/
│   ├── Core/                    # 核心领域层
│   │   ├── Entities/           # 实体定义
│   │   ├── Interfaces/         # 接口定义
│   │   ├── DTOs/              # 数据传输对象
│   │   ├── Exceptions/        # 自定义异常
│   │   └── Services/          # 核心业务逻辑
│   ├── Infrastructure/         # 基础设施层
│   │   ├── Data/              # 数据访问
│   │   ├── Repositories/      # 仓储实现
│   │   ├── Services/          # 外部服务集成
│   │   ├── Messaging/         # 消息处理
│   │   └── External/          # 第三方集成
│   └── WebApi/                # Web API层
│       ├── Controllers/       # 控制器
│       ├── Middleware/        # 中间件
│       ├── Extensions/        # 扩展方法
│       └── Properties/        # 配置文件
└── tests/                     # 测试项目
```

## 快速开始

### 环境要求

- .NET 8 SDK
- Docker & Docker Compose (可选)
- MySQL 8.0+ 或 PostgreSQL

### 开发环境设置

1. 克隆项目
```bash
git clone <repository-url>
cd PetVerse-Api
```

2. 还原NuGet包
```bash
dotnet restore
```

3. 更新数据库连接字符串
编辑 `src/WebApi/PetVerse.Api/appsettings.json` 中的连接字符串

4. 运行数据库迁移
```bash
cd src/Infrastructure/PetVerse.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. 启动应用
```bash
cd src/WebApi/PetVerse.Api
dotnet run
```

应用将在 `http://localhost:5000` 启动

### 使用Docker运行

```bash
docker-compose up -d
```

## API文档

启动应用后，访问 `http://localhost:5000/swagger` 查看API文档

## 配置说明

### 数据库配置
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=petverse_db;Uid=root;Pwd=password;"
  }
}
```

### Redis配置
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### JWT配置
```json
{
  "Jwt": {
    "Key": "your-super-secret-jwt-key-here-minimum-32-characters",
    "Issuer": "PetVerse.Api",
    "Audience": "PetVerse.Users",
    "ExpireMinutes": 1440
  }
}
```

## 主要功能模块

- 用户管理 (注册、登录、个人信息)
- 宠物档案管理
- 社区动态发布
- 宠物定位追踪 (Pettag硬件集成)
- 健康数据监测
- AI图像识别服务

## 开发规范

- 遵循Clean Architecture原则
- 使用CQRS模式处理复杂业务逻辑
- 统一异常处理和响应格式
- 完整的日志记录
- 单元测试覆盖率不低于80%

## 部署

### 生产环境部署

1. 构建发布版本
```bash
dotnet publish -c Release -o ./publish
```

2. 配置生产环境变量
3. 部署到服务器

## 贡献指南

1. Fork项目
2. 创建功能分支
3. 提交更改
4. 发起Pull Request

## 许可证

MIT License