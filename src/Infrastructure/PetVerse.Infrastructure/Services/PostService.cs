using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetVerse.Core.DTOs.Post;
using PetVerse.Core.Entities;
using PetVerse.Core.Exceptions;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Infrastructure.Services
{
    public class PostService(ApplicationDbContext context, IStorageService storageService, IConfiguration configuration, ILogger<PostService> logger) : IPostService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IStorageService _storageService = storageService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<PostService> _logger = logger;
        private readonly int _presignedUrlExpireMinutes = 60;

        public async Task<(List<PostResponse> Posts, int TotalCount)> GetAllPostsAsync(PostQueryParams queryParams)
        {
            try
            {
                IQueryable<Post> query = _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .AsQueryable();

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

                query = query.Where(p => p.Status == 1);

                int totalCount = await query.CountAsync();

                List<Post> posts = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((queryParams.Page - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync();

                var mediaUrlsDict = await GetMediaUrlsForPostsAsync(posts);
                List<PostResponse> postResponses = [.. posts.Select(p => MapToPostResponse(p, mediaUrlsDict))];

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

                var mediaUrlsDict = await GetMediaUrlsForPostsAsync(posts);
                return [.. posts.Select(p => MapToPostResponse(p, mediaUrlsDict))];
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

                var mediaUrlsDict = await GetMediaUrlsForPostsAsync(posts);
                return [.. posts.Select(p => MapToPostResponse(p, mediaUrlsDict))];
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
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
                if (!userExists)
                {
                    throw new NotFoundException($"用户ID {request.UserId} 不存在");
                }

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
                };

                _ = _context.Posts.Add(post);
                _ = await _context.SaveChangesAsync();

                if (request.MediaIds != null && request.MediaIds.Count != 0)
                {
                    post.MediaIds = System.Text.Json.JsonSerializer.Serialize(request.MediaIds);
                    post.MediaCount = (byte)request.MediaIds.Count;
                    _ = await _context.SaveChangesAsync();
                }
                else if (request.MediaItems != null && request.MediaItems.Count != 0)
                {
                    post.MediaCount = (byte)request.MediaItems.Count;
                    _ = await _context.SaveChangesAsync();
                }

                Post createdPost = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .FirstAsync(p => p.Id == post.Id);

                return MapToPostResponse(createdPost);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ValidationException)
            {
                throw;
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
                    .FirstOrDefaultAsync(p => p.Id == id && p.Status == 1) ?? throw new NotFoundException($"帖子ID {id} 不存在");

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
                throw;
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

                post.Status = 0;
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
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
                if (!userExists)
                {
                    throw new NotFoundException($"用户ID {request.UserId} 不存在");
                }

                bool postExists = await _context.Posts.AnyAsync(p => p.Id == request.PostId && p.Status == 1);
                if (!postExists)
                {
                    throw new NotFoundException($"帖子ID {request.PostId} 不存在");
                }

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

                Post? post = await _context.Posts.FindAsync(request.PostId);
                if (post != null)
                {
                    post.CommentsCount++;
                    post.UpdatedAt = DateTime.UtcNow;
                }

                _ = await _context.SaveChangesAsync();

                Comment createdComment = await _context.Comments
                    .Include(c => c.User)
                    .FirstAsync(c => c.Id == comment.Id);

                return MapToCommentInfo(createdComment);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ValidationException)
            {
                throw;
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
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
                if (!userExists)
                {
                    throw new NotFoundException($"用户ID {request.UserId} 不存在");
                }

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

                Like? existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                    l.TargetType == request.TargetType &&
                    l.TargetId == request.TargetId &&
                    l.UserId == request.UserId);

                if (existingLike != null)
                {
                    _ = _context.Likes.Remove(existingLike);

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
                        }
                    }

                    _ = await _context.SaveChangesAsync();
                    return false;
                }
                else
                {
                    Like like = new()
                    {
                        TargetType = request.TargetType,
                        TargetId = request.TargetId,
                        UserId = request.UserId
                    };

                    _ = _context.Likes.Add(like);

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
                        }
                    }

                    _ = await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (NotFoundException)
            {
                throw;
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
                List<Post> posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Pet)
                    .Where(p => p.Status == 1 && p.UserId != userId)
                    .ToListAsync();

                List<Post> recommendedPosts = [.. posts
                    .Select(post => new
                    {
                        Post = post,
                        Score = post.LikesCount + post.CommentsCount +
                                (1 / (1 + (DateTime.UtcNow - post.CreatedAt).TotalDays) * 10)
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(limit)
                    .Select(x => x.Post)];

                var mediaUrlsDict = await GetMediaUrlsForPostsAsync(recommendedPosts);
                return [.. recommendedPosts.Select(p => MapToPostResponse(p, mediaUrlsDict))];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐帖子时发生错误，UserId: {UserId}", userId);
                throw new ServiceException("获取推荐帖子失败", ex);
            }
        }

        private async Task<Dictionary<ulong, List<string>>> GetMediaUrlsForPostsAsync(List<Post> posts)
        {
            var result = new Dictionary<ulong, List<string>>();

            var allMediaIds = new HashSet<ulong>();
            foreach (var post in posts)
            {
                if (!string.IsNullOrEmpty(post.MediaIds))
                {
                    try
                    {
                        var ids = System.Text.Json.JsonSerializer.Deserialize<List<ulong>>(post.MediaIds);
                        if (ids != null)
                        {
                            foreach (var id in ids)
                            {
                                allMediaIds.Add(id);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (allMediaIds.Count == 0)
            {
                return result;
            }

            var mediaResources = await _context.MediaResources
                .Where(m => allMediaIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.StorageKey);

            foreach (var post in posts)
            {
                var urls = new List<string>();
                if (!string.IsNullOrEmpty(post.MediaIds))
                {
                    try
                    {
                        var ids = System.Text.Json.JsonSerializer.Deserialize<List<ulong>>(post.MediaIds);
                        if (ids != null)
                        {
                            foreach (var id in ids)
                            {
                                if (mediaResources.TryGetValue(id, out var storageKey))
                                {
                                    var url = await GetPresignedUrlAsync(storageKey);
                                    urls.Add(url);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                result[post.Id] = urls;
            }

            return result;
        }

        private async Task<string> GetPresignedUrlAsync(string storageKey)
        {
            try
            {
                _logger.LogInformation("正在生成预签名URL: {StorageKey}", storageKey);
                var url = await _storageService.GetFileUrlAsync(storageKey, _presignedUrlExpireMinutes);
                _logger.LogInformation("预签名URL生成成功: {Url}", url);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "生成预签名URL失败，使用降级URL: {StorageKey}", storageKey);
                return GetFallbackUrl(storageKey);
            }
        }

        private string GetFallbackUrl(string storageKey)
        {
            var endpoint = _configuration["Storage:MinIO:Endpoint"] ?? _configuration["MinIO:Endpoint"] ?? "localhost:9000";
            var bucketName = _configuration["Storage:MinIO:BucketName"] ?? _configuration["MinIO:BucketName"] ?? "petverse";
            var useSsl = bool.TryParse(_configuration["Storage:MinIO:UseSSL"], out var ssl) && ssl;

            var protocol = useSsl ? "https" : "http";
            return $"{protocol}://{endpoint}/{bucketName}/{storageKey}";
        }

        private List<string> GetMediaUrls(Post post, Dictionary<ulong, List<string>>? mediaUrlsDict)
        {
            if (mediaUrlsDict != null && mediaUrlsDict.TryGetValue(post.Id, out var urls))
            {
                return urls;
            }
            return [];
        }

        private PostResponse MapToPostResponse(Post post, Dictionary<ulong, List<string>>? mediaUrlsDict = null)
        {
            List<string> mediaUrls = GetMediaUrls(post, mediaUrlsDict);

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
    }
}
