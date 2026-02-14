# 使用官方的 .NET SDK 镜像作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["PetVerse-Api.sln", "."]
COPY ["src/Core/PetVerse.Core/PetVerse.Core.csproj", "src/Core/PetVerse.Core/"]
COPY ["src/Infrastructure/PetVerse.Infrastructure/PetVerse.Infrastructure.csproj", "src/Infrastructure/PetVerse.Infrastructure/"]
COPY ["src/WebApi/PetVerse.Api/PetVerse.Api.csproj", "src/WebApi/PetVerse.Api/"]

# 还原NuGet包
RUN dotnet restore "PetVerse-Api.sln"

# 复制源代码
COPY . .

# 构建项目
WORKDIR "/src/src/WebApi/PetVerse.Api"
RUN dotnet build "PetVerse.Api.csproj" -c Release -o /app/build

# 发布应用
FROM build AS publish
RUN dotnet publish "PetVerse.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 创建非root用户
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# 复制发布文件
COPY --from=publish /app/publish .

# 设置健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "PetVerse.Api.dll"]