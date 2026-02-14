using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PetVerse.Core.Interfaces;
using PetVerse.Core.Models;

namespace PetVerse.Infrastructure.Services;

/// <summary>
/// JWT认证服务实现
/// </summary>
public class JwtAuthService : IJwtAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtAuthService> _logger;

    public JwtAuthService(JwtSettings jwtSettings, ILogger<JwtAuthService> logger)
    {
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public string GenerateToken(ulong userId, string username, List<string> userTags)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, DetermineUserRole(userTags)),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 添加用户标签作为声明
            foreach (var tag in userTags)
            {
                claims.Add(new Claim("tag", tag));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成JWT令牌时发生错误");
            throw;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "验证JWT令牌时发生错误");
            return null;
        }
    }

    public (ulong userId, string username, List<string> userTags)? ExtractClaims(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usernameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(usernameClaim))
                return null;

            var userTags = principal.FindAll("tag").Select(c => c.Value).ToList();

            return (ulong.Parse(userIdClaim), usernameClaim, userTags);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "提取JWT声明时发生错误");
            return null;
        }
    }

    public string RefreshToken(string refreshToken)
    {
        // 简单实现，实际项目中应该有刷新令牌机制
        throw new NotImplementedException("刷新令牌功能暂未实现");
    }

    #region 私有方法

    private string DetermineUserRole(List<string> userTags)
    {
        return userTags.Contains("admin", StringComparer.OrdinalIgnoreCase) ? "Admin" : "User";
    }

    #endregion
}