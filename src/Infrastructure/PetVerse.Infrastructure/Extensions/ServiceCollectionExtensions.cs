using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置数据库连接
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // 注册UnitOfWork
        services.AddScoped<IUnitOfWork, ApplicationDbContext>();
        
        // 注册通用仓储
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // 配置Redis缓存
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "PetVerse";
            });
        }

        return services;
    }
}