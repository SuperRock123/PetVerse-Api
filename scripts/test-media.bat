@echo off
echo ========================================
echo PetVerse 媒体接口测试脚本
echo ========================================

set BASE_URL=http://localhost:5124
set TOKEN=

echo 请先获取认证令牌...
echo 使用以下命令获取令牌：
echo curl -X POST "%BASE_URL%/api/user/login" -H "Content-Type: application/json" -d "{\"usernameOrEmail\":\"testuser\",\"password\":\"password123\"}"
echo.
echo 然后将返回的token设置到TOKEN变量中
echo.

:menu
echo ========================================
echo 请选择要测试的功能:
echo 1. 获取支持的文件类型
echo 2. 获取文件大小限制
echo 3. 上传单个媒体文件 ^(需要TOKEN^)
echo 4. 批量上传媒体文件 ^(需要TOKEN^)
echo 5. 获取帖子媒体文件
echo 6. 获取媒体详情
echo 7. 更新媒体信息 ^(需要TOKEN^)
echo 8. 删除媒体文件 ^(需要TOKEN^)
echo 9. 批量删除媒体文件 ^(需要TOKEN^)
echo 0. 退出
echo ========================================
set /p choice=请输入选项 (0-9): 

if "%choice%"=="1" goto test_supported_types
if "%choice%"=="2" goto test_size_limit
if "%choice%"=="3" goto upload_single
if "%choice%"=="4" goto upload_batch
if "%choice%"=="5" goto get_post_medias
if "%choice%"=="6" goto get_media_detail
if "%choice%"=="7" goto update_media
if "%choice%"=="8" goto delete_media
if "%choice%"=="9" goto delete_medias
if "%choice%"=="0" goto exit

echo 无效选项，请重新选择
goto menu

:test_supported_types
echo 测试获取支持的文件类型...
curl -X GET "%BASE_URL%/api/media/supported-types"
echo.
pause
goto menu

:test_size_limit
echo 测试获取文件大小限制...
curl -X GET "%BASE_URL%/api/media/size-limit"
echo.
pause
goto menu

:upload_single
if "%TOKEN%"=="" (
    echo 请先设置TOKEN变量
    echo 示例: set TOKEN=your-jwt-token-here
    pause
    goto menu
)
echo 准备上传单个媒体文件...
echo 请确保有一个名为 test-image.jpg 的测试文件在当前目录
curl -X POST "%BASE_URL%/api/media" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -F "file=@test-image.jpg" ^
  -F "postId=1" ^
  -F "displayOrder=0"
echo.
pause
goto menu

:upload_batch
if "%TOKEN%"=="" (
    echo 请先设置TOKEN变量
    pause
    goto menu
)
echo 准备批量上传媒体文件...
echo 请确保有 test-image1.jpg 和 test-image2.jpg 测试文件
curl -X POST "%BASE_URL%/api/media/batch" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -F "files=@test-image1.jpg" ^
  -F "files=@test-image2.jpg" ^
  -F "postId=1"
echo.
pause
goto menu

:get_post_medias
echo 获取帖子媒体文件 ^(postId=1^)...
curl -X GET "%BASE_URL%/api/media/post/1"
echo.
pause
goto menu

:get_media_detail
set /p mediaId=请输入媒体ID: 
curl -X GET "%BASE_URL%/api/media/%mediaId%"
echo.
pause
goto menu

:update_media
if "%TOKEN%"=="" (
    echo 请先设置TOKEN变量
    pause
    goto menu
)
set /p mediaId=请输入要更新的媒体ID: 
curl -X PUT "%BASE_URL%/api/media/%mediaId%" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -d "{\"displayOrder\": 1, \"originalName\": \"updated-filename.jpg\"}"
echo.
pause
goto menu

:delete_media
if "%TOKEN%"=="" (
    echo 请先设置TOKEN变量
    pause
    goto menu
)
set /p mediaId=请输入要删除的媒体ID: 
curl -X DELETE "%BASE_URL%/api/media/%mediaId%" ^
  -H "Authorization: Bearer %TOKEN%"
echo.
pause
goto menu

:delete_medias
if "%TOKEN%"=="" (
    echo 请先设置TOKEN变量
    pause
    goto menu
)
echo 批量删除媒体文件...
curl -X DELETE "%BASE_URL%/api/media/batch" ^
  -H "Authorization: Bearer %TOKEN%" ^
  -H "Content-Type: application/json" ^
  -d "[1, 2, 3]"
echo.
pause
goto menu

:exit
echo 再见!
exit /b 0