using Microsoft.AspNetCore.Mvc;
using PetVerse.Api.DTOs.Post;
using PetVerse.Core.DTOs;
using PetVerse.Core.DTOs.Post;
using PetVerse.Core.Interfaces;

namespace PetVerse.Api.Controllers;

/// <summary>
/// 帖子管理控制器
/// </summary>
public class PostController : BaseController
{
    private readonly IPostService _postService;
    private readonly ILogger<PostController> _logger;

    public PostController(IPostService postService, ILogger<PostController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    /// <summary>
    /// 获取帖子列表（分页）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] PostQueryParams queryParams)
    {
        try
        {
            var (posts, totalCount) = await _postService.GetAllPostsAsync(queryParams);
            
            var paginationInfo = new PaginationInfo
            {
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / queryParams.PageSize)
            };

            var result = new
            {
                Posts = posts,
                Pagination = paginationInfo
            };

            return Success(result, "获取帖子列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取帖子列表时发生错误");
            return InternalError("获取帖子列表失败");
        }
    }

    /// <summary>
    /// 根据ID获取帖子详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(ulong id, [FromQuery] ulong? currentUserId = null)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(id, currentUserId);
            
            if (post == null)
                return NotFound("帖子不存在");

            return Success(post, "获取帖子信息成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取帖子信息时发生错误，ID: {PostId}", id);
            return InternalError("获取帖子信息失败");
        }
    }

    /// <summary>
    /// 根据用户ID获取帖子列表
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetPostsByUser(ulong userId, [FromQuery] int limit = 50)
    {
        try
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId, limit);
            return Success(posts, "获取用户帖子列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户帖子列表时发生错误，UserID: {UserId}", userId);
            return InternalError("获取用户帖子列表失败");
        }
    }

    /// <summary>
    /// 根据宠物ID获取帖子列表
    /// </summary>
    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetPostsByPet(ulong petId, [FromQuery] int limit = 50)
    {
        try
        {
            var posts = await _postService.GetPostsByPetIdAsync(petId, limit);
            return Success(posts, "获取宠物帖子列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取宠物帖子列表时发生错误，PetId: {PetId}", petId);
            return InternalError("获取宠物帖子列表失败");
        }
    }

    /// <summary>
    /// 创建帖子
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var post = await _postService.CreatePostAsync(request);
            return Success(post, "帖子创建成功", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建帖子时发生错误");
            return InternalError("创建帖子失败");
        }
    }

    /// <summary>
    /// 更新帖子信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(ulong id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var post = await _postService.UpdatePostAsync(id, request);
            
            if (post == null)
                return NotFound("帖子不存在");

            return Success(post, "帖子信息更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新帖子信息时发生错误，ID: {PostId}", id);
            return InternalError("更新帖子信息失败");
        }
    }

    /// <summary>
    /// 删除帖子（软删除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(ulong id)
    {
        try
        {
            var result = await _postService.DeletePostAsync(id);
            
            if (!result)
                return NotFound("帖子不存在");

            return SuccessNoData("帖子删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除帖子时发生错误，ID: {PostId}", id);
            return InternalError("删除帖子失败");
        }
    }

    /// <summary>
    /// 创建评论
    /// </summary>
    [HttpPost("comment")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var comment = await _postService.CreateCommentAsync(request);
            return Success(comment, "评论创建成功", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建评论时发生错误");
            return InternalError("创建评论失败");
        }
    }

    /// <summary>
    /// 点赞/取消点赞
    /// </summary>
    [HttpPost("like")]
    public async Task<IActionResult> ToggleLike([FromBody] LikeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var isLiked = await _postService.ToggleLikeAsync(request);
            var message = isLiked ? "点赞成功" : "取消点赞成功";
            var result = new { IsLiked = isLiked };
            
            return Success(result, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "点赞操作时发生错误");
            return InternalError("点赞操作失败");
        }
    }

    /// <summary>
    /// 检查帖子是否存在
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<IActionResult> CheckPostExists(ulong id)
    {
        try
        {
            var exists = await _postService.PostExistsAsync(id);
            var result = new { Exists = exists };
            return Success(result, "检查完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查帖子存在性时发生错误，ID: {PostId}", id);
            return InternalError("检查帖子失败");
        }
    }

    /// <summary>
    /// 验证帖子归属关系
    /// </summary>
    [HttpGet("{postId}/belongs-to/{userId}")]
    public async Task<IActionResult> CheckPostOwnership(ulong postId, ulong userId)
    {
        try
        {
            var belongs = await _postService.PostBelongsToUserAsync(postId, userId);
            var result = new { BelongsToUser = belongs };
            return Success(result, "验证完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证帖子归属关系时发生错误，PostId: {PostId}, UserId: {UserId}", postId, userId);
            return InternalError("验证帖子归属关系失败");
        }
    }

    /// <summary>
    /// 上传完整post信息（包含post完整信息和媒体资源批量上传）
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadPost([FromForm] UploadPostRequest request)
    {
        try
        {
            // 获取当前用户ID
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("无效的用户身份");
            }

            // 验证用户ID是否匹配
            if (request.UserId != userId)
            {
                return Forbid("用户ID不匹配");
            }

            // 创建帖子
            var createPostRequest = new CreatePostRequest
            {
                UserId = request.UserId,
                PetId = request.PetId,
                Content = request.Content,
                Location = request.Location,
                Visibility = request.Visibility
            };

            var post = await _postService.CreatePostAsync(createPostRequest);

            // 上传媒体文件并关联到帖子
            if (request.Files != null && request.Files.Any())
            {
                var mediaService = HttpContext.RequestServices.GetRequiredService<IMediaService>();
                var displayOrder = 0;

                foreach (var file in request.Files)
                {
                    using var stream = file.OpenReadStream();
                    await mediaService.UploadMediaAsync(
                        file.FileName,
                        file.ContentType,
                        stream,
                        null, // url_path 为 null
                        userId);
                }
            }

            // 重新获取包含媒体信息的帖子
            var updatedPost = await _postService.GetPostByIdAsync(post.Id);

            return Success(updatedPost, "帖子上传成功", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传帖子时发生错误");
            return InternalError("上传帖子失败");
        }
    }
}