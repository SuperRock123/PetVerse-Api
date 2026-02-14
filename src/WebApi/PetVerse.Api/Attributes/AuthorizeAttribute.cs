using Microsoft.AspNetCore.Mvc.Filters;
using PetVerse.Core.Enums;
using PetVerse.Core.Interfaces;

namespace PetVerse.Api.Attributes;

/// <summary>
/// 自定义授权属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : ActionFilterAttribute
{
    public UserRole RequiredRole { get; set; } = UserRole.User;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        var logger = context.HttpContext.RequestServices.GetService<ILogger<AuthorizeAttribute>>();
        
        if (permissionService == null)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
            {
                success = false,
                message = "权限服务不可用",
                statusCode = 500,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return;
        }

        // 从Claims中获取用户ID
        var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !ulong.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
            {
                success = false,
                message = "无效的用户身份",
                statusCode = 401,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        try
        {
            var hasRequiredRole = await permissionService.HasRoleAsync(userId, RequiredRole);
            
            if (!hasRequiredRole)
            {
                logger?.LogWarning("用户 {UserId} 尝试访问需要 {RequiredRole} 权限的资源", userId, RequiredRole);
                
                context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
                {
                    success = false,
                    message = "权限不足",
                    statusCode = 403,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "权限检查时发生错误");
            
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
            {
                success = false,
                message = "权限检查失败",
                statusCode = 500,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}