using PetVerse.Core.DTOs.Post;

namespace PetVerse.Core.Interfaces
{
    /// <summary>
    /// 帖子服务接口
    /// </summary>
    public interface IPostService
    {
        /// <summary>
        /// 获取所有帖子（分页）
        /// </summary>
        Task<(List<PostResponse> Posts, int TotalCount)> GetAllPostsAsync(PostQueryParams queryParams);

        /// <summary>
        /// 根据ID获取帖子
        /// </summary>
        Task<PostDetailResponse?> GetPostByIdAsync(ulong id, ulong? currentUserId = null);

        /// <summary>
        /// 根据用户ID获取帖子列表
        /// </summary>
        Task<List<PostResponse>> GetPostsByUserIdAsync(ulong userId, int limit = 50);

        /// <summary>
        /// 根据宠物ID获取帖子列表
        /// </summary>
        Task<List<PostResponse>> GetPostsByPetIdAsync(ulong petId, int limit = 50);

        /// <summary>
        /// 创建帖子
        /// </summary>
        Task<PostResponse> CreatePostAsync(CreatePostRequest request);

        /// <summary>
        /// 更新帖子
        /// </summary>
        Task<PostResponse?> UpdatePostAsync(ulong id, UpdatePostRequest request);

        /// <summary>
        /// 删除帖子
        /// </summary>
        Task<bool> DeletePostAsync(ulong id);

        /// <summary>
        /// 创建评论
        /// </summary>
        Task<CommentInfo> CreateCommentAsync(CreateCommentRequest request);

        /// <summary>
        /// 点赞/取消点赞
        /// </summary>
        Task<bool> ToggleLikeAsync(LikeRequest request);

        /// <summary>
        /// 验证帖子是否存在
        /// </summary>
        Task<bool> PostExistsAsync(ulong id);

        /// <summary>
        /// 验证帖子是否属于指定用户
        /// </summary>
        Task<bool> PostBelongsToUserAsync(ulong postId, ulong userId);

        /// <summary>
        /// 获取推荐帖子列表
        /// </summary>
        Task<List<PostResponse>> GetRecommendedPostsAsync(ulong userId, int limit = 20);
    }
}