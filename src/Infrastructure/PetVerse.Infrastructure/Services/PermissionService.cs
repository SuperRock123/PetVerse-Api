using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetVerse.Core.Entities;
using PetVerse.Core.Enums;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Infrastructure.Services;

/// <summary>
/// 权限检查服务实现
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(ApplicationDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasRoleAsync(ulong userId, UserRole requiredRole)
    {
        try
        {
            var userRole = await GetUserRoleAsync(userId);
            return userRole >= requiredRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户角色时发生错误，UserId: {UserId}, Role: {Role}", userId, requiredRole);
            return false;
        }
    }

    public async Task<bool> IsAdminAsync(ulong userId)
    {
        try
        {
            var userTags = await GetUserTagsAsync(userId);
            return userTags.Contains("admin", StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户是否为管理员时发生错误，UserId: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserRole> GetUserRoleAsync(ulong userId)
    {
        try
        {
            var userTags = await GetUserTagsAsync(userId);
            return userTags.Contains("admin", StringComparer.OrdinalIgnoreCase) ? UserRole.Admin : UserRole.User;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户角色时发生错误，UserId: {UserId}", userId);
            return UserRole.User;
        }
    }

    public async Task<List<string>> GetUserTagsAsync(ulong userId)
    {
        try
        {
            var userTags = await _context.UserTagRelations
                .Where(utr => utr.UserId == userId)
                .Include(utr => utr.UserTag)
                .Select(utr => utr.UserTag.TagName)
                .ToListAsync();

            return userTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户标签时发生错误，UserId: {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<bool> HasTagAsync(ulong userId, string tagName)
    {
        try
        {
            return await _context.UserTagRelations
                .AnyAsync(utr => utr.UserId == userId && 
                               utr.UserTag.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户标签时发生错误，UserId: {UserId}, TagName: {TagName}", userId, tagName);
            return false;
        }
    }
}