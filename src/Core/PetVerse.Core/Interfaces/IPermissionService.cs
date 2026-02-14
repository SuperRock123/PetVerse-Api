using PetVerse.Core.Enums;

namespace PetVerse.Core.Interfaces;

/// <summary>
/// 权限检查服务接口
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 检查用户是否具有指定角色
    /// </summary>
    Task<bool> HasRoleAsync(ulong userId, UserRole requiredRole);

    /// <summary>
    /// 检查用户是否是管理员
    /// </summary>
    Task<bool> IsAdminAsync(ulong userId);

    /// <summary>
    /// 获取用户的角色
    /// </summary>
    Task<UserRole> GetUserRoleAsync(ulong userId);

    /// <summary>
    /// 获取用户的标签列表
    /// </summary>
    Task<List<string>> GetUserTagsAsync(ulong userId);

    /// <summary>
    /// 检查用户是否具有指定标签
    /// </summary>
    Task<bool> HasTagAsync(ulong userId, string tagName);
}