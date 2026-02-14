using PetVerse.Core.DTOs.User;
using PetVerse.Core.Entities;

namespace PetVerse.Core.Interfaces;

/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取所有用户（分页）
    /// </summary>
    Task<(List<UserDetailResponse> Users, int TotalCount)> GetAllUsersAsync(UserQueryParams queryParams);

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    Task<UserDetailResponse?> GetUserByIdAsync(ulong id);

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// 创建用户
    /// </summary>
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// 更新用户
    /// </summary>
    Task<UserResponse?> UpdateUserAsync(ulong id, UpdateUserRequest request);

    /// <summary>
    /// 删除用户
    /// </summary>
    Task<bool> DeleteUserAsync(ulong id);

    /// <summary>
    /// 用户登录
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// 验证用户是否存在
    /// </summary>
    Task<bool> UserExistsAsync(ulong id);

    /// <summary>
    /// 验证用户名是否已存在
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// 验证邮箱是否已存在
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// 验证手机号是否已存在
    /// </summary>
    Task<bool> PhoneExistsAsync(string phone);
}