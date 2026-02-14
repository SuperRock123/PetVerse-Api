using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PetVerse.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 注册MediatR
        services.AddMediatR(Assembly.GetExecutingAssembly());
        
        // 注册验证器
        services.AddValidators();
        
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        // 扫描并注册所有验证器
        var validatorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.Name.EndsWith("Validator") && !t.IsAbstract);

        foreach (var validatorType in validatorTypes)
        {
            var interfaceType = validatorType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(FluentValidation.IValidator<>));
            
            if (interfaceType != null)
            {
                services.AddTransient(interfaceType, validatorType);
            }
        }

        return services;
    }
}