namespace PetVerse.Api.Configuration;

/// <summary>
/// 认证配置
/// </summary>
public class AuthConfiguration
{
    /// <summary>
    /// 白名单路由
    /// </summary>
    public List<string> WhitelistRoutes { get; set; } = new();

    /// <summary>
    /// 是否启用全局认证
    /// </summary>
    public bool EnableGlobalAuthentication { get; set; } = true;
}