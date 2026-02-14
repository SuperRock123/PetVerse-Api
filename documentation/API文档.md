# PetVerse API 接口文档

## 基础信息
- 基础URL: `http://localhost:5124/api`
- 响应格式: 统一使用 `ApiResponse<T>` 封装

## 用户相关接口 (UserController)

### GET /api/user
获取用户列表（分页）

**查询参数:**
- `page`: 页码，默认1
- `pageSize`: 每页大小，默认10
- `keyword`: 搜索关键词（用户名/昵称/邮箱）
- `status`: 用户状态（1=正常, 0=禁用, -1=注销）

### GET /api/user/{id}
根据ID获取用户详情

### POST /api/user
创建用户

**请求体:**
```json
{
  "username": "用户名",
  "nickname": "昵称",
  "phone": "手机号",
  "email": "邮箱",
  "password": "密码",
  "avatarUrl": "头像URL",
  "bio": "个人简介"
}
```

### PUT /api/user/{id}
更新用户信息

### DELETE /api/user/{id}
删除用户（软删除）

### POST /api/user/login
用户登录

**请求体:**
```json
{
  "usernameOrEmail": "用户名或邮箱",
  "password": "密码"
}
```

### GET /api/user/check-username/{username}
检查用户名是否可用

### GET /api/user/check-email/{email}
检查邮箱是否可用

## 宠物相关接口 (PetController)

### GET /api/pet
获取宠物列表（分页）

**查询参数:**
- `page`: 页码，默认1
- `pageSize`: 每页大小，默认10
- `userId`: 用户ID筛选
- `keyword`: 搜索关键词（宠物名/品种）
- `breed`: 品种筛选
- `gender`: 性别筛选（0=未知, 1=公, 2=母）

### GET /api/pet/{id}
根据ID获取宠物详情

### GET /api/pet/user/{userId}
根据用户ID获取宠物列表

### POST /api/pet
创建宠物

**请求体:**
```json
{
  "userId": 1,
  "name": "宠物名",
  "breed": "品种",
  "gender": 1,
  "birthday": "2020-01-01",
  "weightKg": 25.5,
  "healthStatus": "健康状况",
  "avatarUrl": "头像URL",
  "pettagId": "宠物标签ID"
}
```

### PUT /api/pet/{id}
更新宠物信息

### DELETE /api/pet/{id}
删除宠物

### GET /api/pet/{id}/exists
检查宠物是否存在

### GET /api/pet/{petId}/belongs-to/{userId}
验证宠物归属关系

## 帖子相关接口 (PostController)

### GET /api/post
获取帖子列表（分页）

**查询参数:**
- `page`: 页码，默认1
- `pageSize`: 每页大小，默认10
- `userId`: 用户ID筛选
- `petId`: 宠物ID筛选
- `keyword`: 搜索关键词
- `visibility`: 可见性（1=公开, 2=仅好友, 3=私密）
- `status`: 状态筛选
- `fromDate`: 开始日期
- `toDate`: 结束日期

### GET /api/post/{id}
根据ID获取帖子详情

**查询参数:**
- `currentUserId`: 当前用户ID（用于判断是否点赞）

### GET /api/post/user/{userId}
根据用户ID获取帖子列表

### GET /api/post/pet/{petId}
根据宠物ID获取帖子列表

### POST /api/post
创建帖子

**请求体:**
```json
{
  "userId": 1,
  "petId": 1,
  "content": "帖子内容",
  "mediaUrls": ["图片URL1", "图片URL2"],
  "location": "位置信息",
  "visibility": 1
}
```

### PUT /api/post/{id}
更新帖子信息

### DELETE /api/post/{id}
删除帖子（软删除）

### POST /api/post/comment
创建评论

**请求体:**
```json
{
  "userId": 1,
  "postId": 1,
  "parentId": 1,
  "content": "评论内容"
}
```

### POST /api/post/like
点赞/取消点赞

**请求体:**
```json
{
  "userId": 1,
  "targetType": "post", // 或 "comment"
  "targetId": 1
}
```

### GET /api/post/{id}/exists
检查帖子是否存在

### GET /api/post/{postId}/belongs-to/{userId}
验证帖子归属关系

## 统一响应格式

所有API响应都遵循以下格式：

```json
{
  "success": true,
  "message": "操作成功",
  "data": {},
  "errors": [],
  "statusCode": 200,
  "timestamp": "2026-02-14T15:00:00Z"
}
```

## 错误码说明

- 200: 成功
- 400: 请求参数错误
- 401: 未授权
- 404: 资源不存在
- 409: 资源冲突
- 500: 服务器内部错误