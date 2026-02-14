@echo off
echo PetVerse API 测试脚本
echo ====================

set BASE_URL=http://localhost:5124

echo 测试1: 获取用户列表
curl -X GET "%BASE_URL%/api/user?page=1&pageSize=10" -H "Content-Type: application/json"
echo.
echo.

echo 测试2: 创建用户
curl -X POST "%BASE_URL%/api/user" -H "Content-Type: application/json" -d "{\"username\":\"testuser\",\"nickname\":\"测试用户\",\"email\":\"test@example.com\",\"password\":\"password123\",\"bio\":\"这是测试用户\"}"
echo.
echo.

echo 测试3: 获取宠物列表
curl -X GET "%BASE_URL%/api/pet?page=1&pageSize=10" -H "Content-Type: application/json"
echo.
echo.

echo 测试4: 创建宠物
curl -X POST "%BASE_URL%/api/pet" -H "Content-Type: application/json" -d "{\"userId\":1,\"name\":\"小白\",\"breed\":\"金毛\",\"gender\":1,\"birthday\":\"2020-01-01\",\"weightKg\":25.5,\"healthStatus\":\"健康\"}"
echo.
echo.

echo 测试5: 获取帖子列表
curl -X GET "%BASE_URL%/api/post?page=1&pageSize=10" -H "Content-Type: application/json"
echo.
echo.

echo 测试6: 用户登录
curl -X POST "%BASE_URL%/api/user/login" -H "Content-Type: application/json" -d "{\"usernameOrEmail\":\"testuser\",\"password\":\"password123\"}"
echo.
echo.

echo 所有测试完成！
pause