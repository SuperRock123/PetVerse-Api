using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetVerse.Core.DTOs.Post;
using PetVerse.Core.Entities;
using PetVerse.Core.Exceptions;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Infrastructure.Services
{
    /// <summary>
    /// 帖子服务实现
    /// </summary>
    public class PostService(ApplicationDbContext context, ILogger<PostService> logger) : IPostService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<PostService> _logger = logger;

        public async Task<(List<PostResponse> Posts, int TotalCount)> GetAllPostsAsync(PostQueryParams queryParams)
        {
            try
            {
                IQueryable<Post> query = _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .AsQueryable();

                // 应用查询条件
                if (queryParams.UserId.HasValue)
                {
                    query = query.Where(p => p.UserId == queryParams.UserId.Value);
                }

                if (queryParams.PetId.HasValue)
                {
                    query = query.Where(p => p.PetId == queryParams.PetId.Value);
                }

                if (!string.IsNullOrEmpty(queryParams.Keyword))
                {
                    query = query.Where(p => p.Content.Contains(queryParams.Keyword));
                }

                if (queryParams.Visibility.HasValue)
                {
                    query = query.Where(p => p.Visibility == queryParams.Visibility.Value);
                }

                if (queryParams.Status.HasValue)
                {
                    query = query.Where(p => p.Status == queryParams.Status.Value);
                }

                if (queryParams.FromDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt >= queryParams.FromDate.Value);
                }

                if (queryParams.ToDate.HasValue)
                {
                    query = query.Where(p => p.CreatedAt <= queryParams.ToDate.Value);
                }

                // 只显示正常状态的帖子
                query = query.Where(p => p.Status == 1);

                // 获取总数
                int totalCount = await query.CountAsync();

                // 应用分页
                List<Post> posts = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((queryParams.Page - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync();

                List<PostResponse> postResponses = [.. posts.Select(MapToPostResponse)];

                return (postResponses, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取帖子列表时发生错误");
                throw new ServiceException("获取帖子列表失败", ex);
            }
        }

        public async Task<PostDetailResponse?> GetPostByIdAsync(ulong id, ulong? currentUserId = null)
        {
            try
            {
                Post? post = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Status == 1);

                if (post == null)
                {
                    return null;
                }

                PostDetailResponse response = MapToPostDetailResponse(post);

                // 检查当前用户是否点赞
                if (currentUserId.HasValue)
                {
                    response.IsLiked = await _context.Likes.AnyAsync(l =>
                        l.TargetType == "post" &&
                        l.TargetId == id &&
                        l.UserId == currentUserId.Value);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据ID获取帖子时发生错误，ID: {PostId}", id);
                throw new ServiceException("获取帖子信息失败", ex);
            }
        }

        public async Task<List<PostResponse>> GetPostsByUserIdAsync(ulong userId, int limit = 50)
        {
            try
            {
                List<Post> posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .Where(p => p.UserId == userId && p.Status == 1)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                return [.. posts.Select(MapToPostResponse)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据用户ID获取帖子列表时发生错误，UserID: {UserId}", userId);
                throw new ServiceException("获取帖子列表失败", ex);
            }
        }

        public async Task<List<PostResponse>> GetPostsByPetIdAsync(ulong petId, int limit = 50)
        {
            try
            {
                List<Post> posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .Where(p => p.PetId == petId && p.Status == 1)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(limit)
                    .ToListAsync();

                return [.. posts.Select(MapToPostResponse)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据宠物ID获取帖子列表时发生错误，PetId: {PetId}", petId);
                throw new ServiceException("获取帖子列表失败", ex);
            }
        }

        public async Task<PostResponse> CreatePostAsync(CreatePostRequest request)
        {
            try
            {
                // 验证用户是否存在
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
                if (!userExists)
                {
                    throw new NotFoundException($"用户ID {request.UserId} 不存在");
                }

                // 如果指定了宠物ID，验证宠物是否存在且属于该用户
                if (request.PetId.HasValue)
                {
                    bool petExists = await _context.Pets.AnyAsync(p =>
                        p.Id == request.PetId.Value && p.UserId == request.UserId);
                    if (!petExists)
                    {
                        throw new ValidationException("指定的宠物不属于该用户");
                    }
                }

                Post post = new()
                {
                    UserId = request.UserId,
                    PetId = request.PetId,
                    Content = request.Content,
                    Location = request.Location,
                    Visibility = request.Visibility,
                    Status = 1,
                    // MediaCount 是生成列，不需要手动设置
                };

                _ = _context.Posts.Add(post);
                _ = await _context.SaveChangesAsync();

                // 处理媒体资源
                if (request.MediaItems != null && request.MediaItems.Count != 0)
                {
                    // 这里应该将媒体资源ID存储到post的media_ids字段中
                    // 但由于我们没有修改Post实体的结构，暂时跳过这部分
                    // 实际项目中需要在Post实体中添加media_ids字段
                    post.MediaCount = (byte)request.MediaItems.Count;
                    _ = await _context.SaveChangesAsync();
                }

                // 重新加载包含关联信息的数据
                Post createdPost = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .FirstAsync(p => p.Id == post.Id);

                return MapToPostResponse(createdPost);
            }
            catch (NotFoundException)
            {
                throw; // 重新抛出未找到异常
            }
            catch (ValidationException)
            {
                throw; // 重新抛出验证异常
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建帖子时发生错误");
                throw new ServiceException("创建帖子失败", ex);
            }
        }

        public async Task<PostResponse?> UpdatePostAsync(ulong id, UpdatePostRequest request)
        {
            try
            {
                Post post = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    //TODO 这个有问题
                    // .Include(p => p.MediaItems)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Status == 1) ?? throw new NotFoundException($"帖子ID {id} 不存在");

                // 更新帖子信息
                if (request.Content != null)
                {
                    post.Content = request.Content;
                }

                if (request.Location != null)
                {
                    post.Location = request.Location;
                }

                if (request.Visibility.HasValue)
                {
                    post.Visibility = request.Visibility.Value;
                }

                // 更新媒体数量
                if (request.MediaItems != null)
                {
                    post.MediaCount = (byte?)request.MediaItems.Count;
                }

                post.UpdatedAt = DateTime.UtcNow;

                _ = await _context.SaveChangesAsync();
                return MapToPostResponse(post);
            }
            catch (NotFoundException)
            {
                throw; // 重新抛出未找到异常
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新帖子时发生错误，ID: {PostId}", id);
                throw new ServiceException("更新帖子失败", ex);
            }
        }

        public async Task<bool> DeletePostAsync(ulong id)
        {
            try
            {
                Post? post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return false;
                }

                post.Status = 0; // 软删除
                post.UpdatedAt = DateTime.UtcNow;

                _ = await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除帖子时发生错误，ID: {PostId}", id);
                throw new ServiceException("删除帖子失败", ex);
            }
        }

        public async Task<CommentInfo> CreateCommentAsync(CreateCommentRequest request)
        {
            try
            {
                // 验证用户是否存在
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
                if (!userExists)
                {
                    throw new NotFoundException($"用户ID {request.UserId} 不存在");
                }

                // 验证帖子是否存在
                bool postExists = await _context.Posts.AnyAsync(p => p.Id == request.PostId && p.Status == 1);
                if (!postExists)
                {
                    throw new NotFoundException($"帖子ID {request.PostId} 不存在");
                }

                // 如果指定了父评论，验证父评论是否存在
                if (request.ParentId.HasValue)
                {
                    bool parentCommentExists = await _context.Comments.AnyAsync(c =>
                        c.Id == request.ParentId.Value && c.PostId == request.PostId);
                    if (!parentCommentExists)
                    {
                        throw new ValidationException("父评论不存在或不属于该帖子");
                    }
                }

                Comment comment = new()
                {
                    PostId = request.PostId,
                    UserId = request.UserId,
                    ParentId = request.ParentId,
                    Content = request.Content,
                    Status = 1
                };

                _ = _context.Comments.Add(comment);

                // 更新帖子的评论数
                Post? post = await _context.Posts.FindAsync(request.PostId);
                if (post != null)
                {
                    post.CommentsCount++;
                    post.UpdatedAt = DateTime.UtcNow;
                }

                _ = await _context.SaveChangesAsync();

                // 重新加载包含用户信息的数据
                Comment createdComment = await _context.Comments
                    .Include(c => c.User)
                    .FirstAsync(c => c.Id == comment.Id);

                return MapToCommentInfo(createdComment);
            }
            catch (NotFoundException)
            {
                throw; // 重新抛出未找到异常
            }
            catch (ValidationException)
            {
                throw; // 重新抛出验证异常
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评论时发生错误");
                throw new ServiceException("创建评论失败", ex);
            }
        }

        public async Task<bool> ToggleLikeAsync(LikeRequest request)
        {
            try
            {
                // 验证用户是否存在
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
                if (!userExists)
                {
                    throw new NotFoundException($"用户ID {request.UserId} 不存在");
                }

                // 验证目标是否存在
                bool targetExists = false;
                if (request.TargetType == "post")
                {
                    targetExists = await _context.Posts.AnyAsync(p => p.Id == request.TargetId && p.Status == 1);
                }
                else if (request.TargetType == "comment")
                {
                    targetExists = await _context.Comments.AnyAsync(c => c.Id == request.TargetId && c.Status == 1);
                }

                if (!targetExists)
                {
                    throw new NotFoundException($"目标 {request.TargetType} ID {request.TargetId} 不存在");
                }

                // 检查是否已点赞
                Like? existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                    l.TargetType == request.TargetType &&
                    l.TargetId == request.TargetId &&
                    l.UserId == request.UserId);

                if (existingLike != null)
                {
                    // 取消点赞
                    _ = _context.Likes.Remove(existingLike);

                    // 更新点赞数
                    if (request.TargetType == "post")
                    {
                        Post? post = await _context.Posts.FindAsync(request.TargetId);
                        if (post != null && post.LikesCount > 0)
                        {
                            post.LikesCount--;
                            post.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else if (request.TargetType == "comment")
                    {
                        Comment? comment = await _context.Comments.FindAsync(request.TargetId);
                        if (comment != null && comment.LikesCount > 0)
                        {
                            comment.LikesCount--;
                            // Comment实体没有UpdatedAt字段，跳过更新
                        }
                    }

                    _ = await _context.SaveChangesAsync();
                    return false; // 表示取消点赞
                }
                else
                {
                    // 添加点赞
                    Like like = new()
                    {
                        TargetType = request.TargetType,
                        TargetId = request.TargetId,
                        UserId = request.UserId
                    };

                    _ = _context.Likes.Add(like);

                    // 更新点赞数
                    if (request.TargetType == "post")
                    {
                        Post? post = await _context.Posts.FindAsync(request.TargetId);
                        if (post != null)
                        {
                            post.LikesCount++;
                            post.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else if (request.TargetType == "comment")
                    {
                        Comment? comment = await _context.Comments.FindAsync(request.TargetId);
                        if (comment != null)
                        {
                            comment.LikesCount++;
                            // Comment实体没有UpdatedAt字段，跳过更新
                        }
                    }

                    _ = await _context.SaveChangesAsync();
                    return true; // 表示新增点赞
                }
            }
            catch (NotFoundException)
            {
                throw; // 重新抛出未找到异常
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "点赞操作时发生错误");
                throw new ServiceException("点赞操作失败", ex);
            }
        }

        public async Task<bool> PostExistsAsync(ulong id)
        {
            try
            {
                return await _context.Posts.AnyAsync(p => p.Id == id && p.Status == 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证帖子存在性时发生错误，ID: {PostId}", id);
                throw new ServiceException("验证帖子失败", ex);
            }
        }

        public async Task<bool> PostBelongsToUserAsync(ulong postId, ulong userId)
        {
            try
            {
                return await _context.Posts.AnyAsync(p => p.Id == postId && p.UserId == userId && p.Status == 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证帖子归属关系时发生错误，PostId: {PostId}, UserId: {UserId}", postId, userId);
                throw new ServiceException("验证帖子归属关系失败", ex);
            }
        }

        public async Task<List<PostResponse>> GetRecommendedPostsAsync(ulong userId, int limit = 20)
        {
            try
            {
                // 查询所有正常状态的帖子，排除当前用户自己的帖子
                List<Post> posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .Where(p => p.Status == 1 && p.UserId != userId)
                    .ToListAsync();

                // 简单的推荐算法：基于互动度（点赞数 + 评论数）和新鲜度（发布时间）
                List<Post> recommendedPosts = [.. posts
                    .Select(post => new
                    {
                        Post = post,
                        // 计算推荐分数：互动度 + 新鲜度权重
                        Score = post.LikesCount + post.CommentsCount +
                                (1 / (1 + (DateTime.UtcNow - post.CreatedAt).TotalDays) * 10)
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(limit)
                    .Select(x => x.Post)];

                return [.. recommendedPosts.Select(MapToPostResponse)];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐帖子时发生错误，UserId: {UserId}", userId);
                throw new ServiceException("获取推荐帖子失败", ex);
            }
        }

        #region 私有方法

        private PostResponse MapToPostResponse(Post post)
        {
            // 由于我们没有修改Post实体添加media_ids字段，暂时返回空的媒体URL列表
            // 实际项目中需要从media_ids字段解析媒体资源ID，然后从media_resources表获取URL
            List<string> mediaUrls = [];

            return new PostResponse
            {
                Id = post.Id,
                UserId = post.UserId,
                PetId = post.PetId,
                Content = post.Content,
                MediaUrls = mediaUrls,
                Location = post.Location,
                LikesCount = post.LikesCount,
                CommentsCount = post.CommentsCount,
                ViewCount = post.ViewCount,
                MediaCount = post.MediaCount,
                Visibility = post.Visibility,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                UserName = post.User?.Username ?? "",
                UserAvatar = post.User?.AvatarUrl,
                Nickname = post.User?.Nickname,
                PetName = post.Pet?.Name,
                PetAvatar = post.Pet?.AvatarUrl
            };
        }

        private PostDetailResponse MapToPostDetailResponse(Post post)
        {
            PostResponse baseResponse = MapToPostResponse(post);
            PostDetailResponse response = new()
            {
                Id = baseResponse.Id,
                UserId = baseResponse.UserId,
                PetId = baseResponse.PetId,
                Content = baseResponse.Content,
                MediaUrls = baseResponse.MediaUrls,
                Location = baseResponse.Location,
                LikesCount = baseResponse.LikesCount,
                CommentsCount = baseResponse.CommentsCount,
                Visibility = baseResponse.Visibility,
                Status = baseResponse.Status,
                CreatedAt = baseResponse.CreatedAt,
                UpdatedAt = baseResponse.UpdatedAt,
                UserName = baseResponse.UserName,
                UserAvatar = baseResponse.UserAvatar,
                Nickname = baseResponse.Nickname,
                PetName = baseResponse.PetName,
                PetAvatar = baseResponse.PetAvatar,
                Comments = [.. post.Comments
                    .Where(static c => c.Status == 1)
                    .Select(MapToCommentInfo)]
            };

            return response;
        }

        private CommentInfo MapToCommentInfo(Comment comment)
        {
            return new CommentInfo
            {
                Id = comment.Id,
                UserId = comment.UserId,
                Content = comment.Content,
                LikesCount = comment.LikesCount,
                CreatedAt = comment.CreatedAt,
                UserName = comment.User?.Username ?? "",
                UserAvatar = comment.User?.AvatarUrl,
                Nickname = comment.User?.Nickname
            };
        }

        #endregion
    }
}