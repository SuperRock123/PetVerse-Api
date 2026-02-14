using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetVerse.Core.DTOs.User;
using PetVerse.Core.Entities;
using PetVerse.Core.Exceptions;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;
using BCrypt.Net;

namespace PetVerse.Infrastructure.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IPermissionService _permissionService;

    public UserService(ApplicationDbContext context, 
        ILogger<UserService> logger,
        IJwtAuthService jwtAuthService,
        IPermissionService permissionService)
    {
        _context = context;
        _logger = logger;
        _jwtAuthService = jwtAuthService;
        _permissionService = permissionService;
    }

    public async Task<(List<UserDetailResponse> Users, int TotalCount)> GetAllUsersAsync(UserQueryParams queryParams)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            // 应用查询条件
            if (!string.IsNullOrEmpty(queryParams.Keyword))
            {
                query = query.Where(u => u.Username.Contains(queryParams.Keyword) || 
                                       u.Nickname.Contains(queryParams.Keyword) ||
                                       u.Email.Contains(queryParams.Keyword));
            }

            if (queryParams.Status.HasValue)
            {
                query = query.Where(u => u.Status == queryParams.Status.Value);
            }

            // 获取总数
            var totalCount = await query.CountAsync();

            // 应用分页
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Include(u => u.Pets)
                .Include(u => u.Posts)
                .Include(u => u.UserTagRelations)
                    .ThenInclude(utr => utr.UserTag)
                .ToListAsync();

            var userResponses = users.Select(MapToUserDetailResponse).ToList();

            return (userResponses, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户列表时发生错误");
            throw new ServiceException("获取用户列表失败", ex);
        }
    }

    public async Task<UserDetailResponse?> GetUserByIdAsync(ulong id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Pets)
                .Include(u => u.Posts)
                .Include(u => u.UserTagRelations)
                    .ThenInclude(utr => utr.UserTag)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : MapToUserDetailResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据ID获取用户时发生错误，ID: {UserId}", id);
            throw new ServiceException("获取用户信息失败", ex);
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据用户名获取用户时发生错误，Username: {Username}", username);
            throw new ServiceException("查询用户失败", ex);
        }
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // 验证唯一性
            if (await UsernameExistsAsync(request.Username))
                throw new ConflictException($"用户名 '{request.Username}' 已存在");

            if (!string.IsNullOrEmpty(request.Email) && await EmailExistsAsync(request.Email))
                throw new ConflictException($"邮箱 '{request.Email}' 已存在");

            if (!string.IsNullOrEmpty(request.Phone) && await PhoneExistsAsync(request.Phone))
                throw new ConflictException($"手机号 '{request.Phone}' 已存在");

            var user = new User
            {
                Username = request.Username,
                Nickname = request.Nickname,
                Phone = request.Phone,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                AvatarUrl = request.AvatarUrl,
                Bio = request.Bio,
                Status = 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToUserResponse(user);
        }
        catch (ConflictException)
        {
            throw; // 重新抛出冲突异常
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户时发生错误");
            throw new ServiceException("创建用户失败", ex);
        }
    }

    public async Task<UserResponse?> UpdateUserAsync(ulong id, UpdateUserRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new NotFoundException($"用户ID {id} 不存在");

            // 验证唯一性（排除当前用户）
            if (!string.IsNullOrEmpty(request.Email) && 
                await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                throw new ConflictException($"邮箱 '{request.Email}' 已被其他用户使用");

            if (!string.IsNullOrEmpty(request.Phone) && 
                await _context.Users.AnyAsync(u => u.Phone == request.Phone && u.Id != id))
                throw new ConflictException($"手机号 '{request.Phone}' 已被其他用户使用");

            // 更新用户信息
            if (request.Nickname != null) user.Nickname = request.Nickname;
            if (request.Phone != null) user.Phone = request.Phone;
            if (request.Email != null) user.Email = request.Email;
            if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
            if (request.Bio != null) user.Bio = request.Bio;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToUserResponse(user);
        }
        catch (NotFoundException)
        {
            throw; // 重新抛出未找到异常
        }
        catch (ConflictException)
        {
            throw; // 重新抛出冲突异常
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用户时发生错误，ID: {UserId}", id);
            throw new ServiceException("更新用户失败", ex);
        }
    }

    public async Task<bool> DeleteUserAsync(ulong id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.Status = -1; // 软删除
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除用户时发生错误，ID: {UserId}", id);
            throw new ServiceException("删除用户失败", ex);
        }
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null)
                return null;

            if (user.Status != 1)
                throw new ValidationException("用户账户已被禁用");

            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new ValidationException("用户未设置密码");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            // 更新最后登录时间
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 获取用户标签和角色
            var userTags = await _permissionService.GetUserTagsAsync(user.Id);
            var userRole = await _permissionService.GetUserRoleAsync(user.Id);

            return new LoginResponse
            {
                Token = _jwtAuthService.GenerateToken(user.Id, user.Username, userTags),
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                User = MapToUserResponse(user, userRole, userTags),
                Role = userRole.ToString(),
                Tags = userTags
            };
        }
        catch (ValidationException)
        {
            throw; // 重新抛出验证异常
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户登录时发生错误");
            throw new ServiceException("登录失败", ex);
        }
    }

    public async Task<bool> UserExistsAsync(ulong id)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Id == id && u.Status == 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证用户存在性时发生错误，ID: {UserId}", id);
            throw new ServiceException("验证用户失败", ex);
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Username == username && u.Status == 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证用户名存在性时发生错误，Username: {Username}", username);
            throw new ServiceException("验证用户名失败", ex);
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Status == 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证邮箱存在性时发生错误，Email: {Email}", email);
            throw new ServiceException("验证邮箱失败", ex);
        }
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone && u.Status == 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证手机号存在性时发生错误，Phone: {Phone}", phone);
            throw new ServiceException("验证手机号失败", ex);
        }
    }

    #region 私有方法

    private UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            Phone = user.Phone,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Status = user.Status
        };
    }

    private UserResponse MapToUserResponse(User user, Core.Enums.UserRole role, List<string> tags)
    {
        var response = MapToUserResponse(user);
        response.Role = role.ToString();
        response.Tags = tags;
        return response;
    }

    private UserDetailResponse MapToUserDetailResponse(User user)
    {
        var response = new UserDetailResponse
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            Phone = user.Phone,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Status = user.Status,
            PetsCount = user.Pets.Count,
            PostsCount = user.Posts.Count,
            Tags = user.UserTagRelations.Select(utr => utr.UserTag.TagName).ToList()
        };

        return response;
    }

    #endregion
}