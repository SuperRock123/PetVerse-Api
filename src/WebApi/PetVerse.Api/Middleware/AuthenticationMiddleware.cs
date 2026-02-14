using System.Security.Claims;
using PetVerse.Api.Configuration;
using PetVerse.Core.Interfaces;

namespace PetVerse.Api.Middleware;

/// <summary>
/// 认证中间件
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly AuthConfiguration _authConfig;

    public AuthenticationMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        
        _authConfig = new AuthConfiguration();
        configuration.GetSection("Auth").Bind(_authConfig);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 从服务提供者获取Scoped服务
        var jwtAuthService = context.RequestServices.GetRequiredService<IJwtAuthService>();
        
        // 检查是否启用全局认证
        if (!_authConfig.EnableGlobalAuthentication)
        {
            await _next(context);
            return;
        }

        // 检查是否在白名单中
        if (IsWhitelisted(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // 从Authorization头获取令牌
        var token = ExtractTokenFromHeader(context.Request.Headers.Authorization);
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("请求缺少认证令牌: {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "未提供认证令牌",
                statusCode = 401,
                timestamp = DateTime.UtcNow
            });
            return;
        }

        // 验证令牌
        var claimsPrincipal = jwtAuthService.ValidateToken(token);
        if (claimsPrincipal == null)
        {
            _logger.LogWarning("无效的认证令牌: {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "无效的认证令牌",
                statusCode = 401,
                timestamp = DateTime.UtcNow
            });
            return;
        }

        // 将用户信息添加到HttpContext
        context.User = claimsPrincipal;
        
        await _next(context);
    }

    #region 私有方法

    private bool IsWhitelisted(string path)
    {
        var normalizedPath = path.ToLowerInvariant();
        
        return _authConfig.WhitelistRoutes.Any(route => 
            normalizedPath.StartsWith(route.ToLowerInvariant()) ||
            MatchesWildcardPattern(normalizedPath, route.ToLowerInvariant()));
    }

    private bool MatchesWildcardPattern(string path, string pattern)
    {
        if (pattern.EndsWith("*"))
        {
            var prefix = pattern.TrimEnd('*');
            return path.StartsWith(prefix);
        }
        return path.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private string? ExtractTokenFromHeader(string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader))
            return null;

        const string bearerPrefix = "Bearer ";
        if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader.Substring(bearerPrefix.Length).Trim();
        }

        return null;
    }

    #endregion
}

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}