using Serilog;
using PetVerse.Infrastructure.Extensions;
using PetVerse.Api.Extensions;
using PetVerse.Api.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PetVerse API",
        Description = "宠物社交平台API文档"
    });

    // 配置JWT认证
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "请输入JWT Token，格式: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add Application Services
builder.Services.AddApplicationServices();

// Add Custom Services
builder.Services.AddScoped<PetVerse.Core.Interfaces.IUserService, PetVerse.Infrastructure.Services.UserService>();
builder.Services.AddScoped<PetVerse.Core.Interfaces.IPetService, PetVerse.Infrastructure.Services.PetService>();
builder.Services.AddScoped<PetVerse.Core.Interfaces.IPostService, PetVerse.Infrastructure.Services.PostService>();
builder.Services.AddScoped<PetVerse.Core.Interfaces.IMediaService, PetVerse.Infrastructure.Services.MediaService>();
builder.Services.AddScoped<PetVerse.Core.Interfaces.IJwtAuthService, PetVerse.Infrastructure.Services.JwtAuthService>();
builder.Services.AddScoped<PetVerse.Core.Interfaces.IPermissionService, PetVerse.Infrastructure.Services.PermissionService>();

// Add Infrastructure Services
builder.Services.AddInfrastructure(builder.Configuration);

// 注册存储服务（使用内存存储进行开发测试）
builder.Services.AddScoped<PetVerse.Core.Interfaces.IStorageService, PetVerse.Infrastructure.Services.InMemoryStorageService>();

// Add Health Checks
builder.Services.AddHealthChecks();

// Configure JWT Settings
builder.Services.Configure<PetVerse.Core.Models.JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Register JwtSettings as singleton
builder.Services.AddSingleton(provider =>
    provider.GetRequiredService<IConfiguration>().GetSection("JwtSettings").Get<PetVerse.Core.Models.JwtSettings>() ?? new PetVerse.Core.Models.JwtSettings());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用全局异常处理中间件
app.UseGlobalExceptionMiddleware();

// 使用自定义认证中间件
app.UseCustomAuthentication();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

app.Run();
