using System.Security.Claims;

namespace PetVerse.Core.Interfaces;

/// <summary>
/// JWT认证服务接口
/// </summary>
public interface IJwtAuthService
{
    /// <summary>
    /// 生成JWT令牌
    /// </summary>
    string GenerateToken(ulong userId, string username, List<string> userTags);

    /// <summary>
    /// 验证JWT令牌
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// 从令牌中提取用户声明
    /// </summary>
    (ulong userId, string username, List<string> userTags)? ExtractClaims(string token);

    /// <summary>
    /// 刷新JWT令牌
    /// </summary>
    string RefreshToken(string refreshToken);
}