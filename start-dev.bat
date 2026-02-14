@echo off
echo 正在启动PetVerse API开发环境...

REM 检查Docker是否运行
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo 错误: Docker未运行，请先启动Docker Desktop
    pause
    exit /b 1
)

REM 构建并启动所有服务
echo 正在构建和启动Docker容器...
docker-compose up -d --build

if %errorlevel% equ 0 (
    echo.
    echo PetVerse API 已成功启动！
    echo.
    echo 服务地址:
    echo API: http://localhost:5000
    echo Swagger文档: http://localhost:5000/swagger
    echo MySQL: localhost:3306
    echo Redis: localhost:6379
    echo RabbitMQ管理界面: http://localhost:15672
    echo MinIO控制台: http://localhost:9001
    echo EMQX Dashboard: http://localhost:18083
    echo.
    echo 使用以下命令查看日志:
    echo docker-compose logs -f api
    echo.
    echo 使用以下命令停止服务:
    echo docker-compose down
) else (
    echo 启动失败，请检查错误信息
)

pause