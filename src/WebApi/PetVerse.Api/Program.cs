using Serilog;
using PetVerse.Infrastructure.Extensions;
using PetVerse.Api.Extensions;
using PetVerse.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Services
builder.Services.AddApplicationServices();

// Add Infrastructure Services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用全局异常处理中间件
app.UseGlobalExceptionMiddleware();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

app.Run();
