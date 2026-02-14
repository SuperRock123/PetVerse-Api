@echo off
echo PetVerse API 认证功能测试脚本
echo ==================================

set BASE_URL=http://localhost:5124

echo 1. 测试白名单接口（无需认证）
echo 测试登录接口:
curl -X POST "%BASE_URL%/api/user/login" -H "Content-Type: application/json" -d "{\"usernameOrEmail\":\"testuser\",\"password\":\"password123\"}"
echo.
echo.

echo 测试用户名检查接口:
curl -X GET "%BASE_URL%/api/user/check-username/testuser"
echo.
echo.

echo 测试健康检查接口:
curl -X GET "%BASE_URL%/health"
echo.
echo.

echo 2. 测试需要认证的接口（应该返回401）
echo 测试获取用户列表（无令牌）:
curl -X GET "%BASE_URL%/api/user?page=1&pageSize=10" -H "Content-Type: application/json"
echo.
echo.

echo 测试获取宠物列表（无令牌）:
curl -X GET "%BASE_URL%/api/pet?page=1&pageSize=10" -H "Content-Type: application/json"
echo.
echo.

echo 3. 完整认证流程测试
echo 创建测试用户...
curl -X POST "%BASE_URL%/api/user" -H "Content-Type: application/json" -d "{\"username\":\"authtest\",\"nickname\":\"认证测试用户\",\"email\":\"auth@test.com\",\"password\":\"test123\",\"bio\":\"用于认证测试的用户\"}"
echo.
echo.

echo 用户登录获取JWT令牌...
for /f "tokens=*" %%i in ('curl -s -X POST "%BASE_URL%/api/user/login" -H "Content-Type: application/json" -d "{\"usernameOrEmail\":\"authtest\",\"password\":\"test123\"}" ^| jq -r ".data.token"') do set JWT_TOKEN=%%i

echo 获取到的JWT令牌: %JWT_TOKEN%
echo.
echo.

echo 使用JWT令牌访问受保护接口...
echo 获取用户列表（带令牌）:
curl -X GET "%BASE_URL%/api/user?page=1&pageSize=10" -H "Content-Type: application/json" -H "Authorization: Bearer %JWT_TOKEN%"
echo.
echo.

echo 获取宠物列表（带令牌）:
curl -X GET "%BASE_URL%/api/pet?page=1&pageSize=10" -H "Content-Type: application/json" -H "Authorization: Bearer %JWT_TOKEN%"
echo.
echo.

echo 所有测试完成！
pause