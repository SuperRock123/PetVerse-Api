namespace PetVerse.Core.Models;

/// <summary>
/// JWT配置设置
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// 密钥
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 发行者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（分钟）
    /// </summary>
    public int ExpirationMinutes { get; set; } = 120;

    /// <summary>
    /// 刷新令牌过期时间（天）
    /// </summary>
    public int RefreshExpirationDays { get; set; } = 7;
}