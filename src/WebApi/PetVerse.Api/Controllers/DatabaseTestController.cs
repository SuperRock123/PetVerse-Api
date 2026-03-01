using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetVerse.Core.Entities;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseTestController : BaseController
{
    private readonly ApplicationDbContext _context;

    public DatabaseTestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            // 测试数据库连接
            var canConnect = await _context.Database.CanConnectAsync();
            
            // 获取用户表数量
            var userCount = await _context.Users.CountAsync();
            
            // 获取宠物表数量
            var petCount = await _context.Pets.CountAsync();
            
            var result = new
            {
                CanConnect = canConnect,
                UserCount = userCount,
                PetCount = petCount,
                DatabaseName = "petverse"
            };
            
            return Success(result, "数据库连接成功");
        }
        catch (Exception ex)
        {
            return Error("数据库连接失败", new List<string> { ex.Message });
        }
    }

    [HttpGet("tables")]
    public async Task<IActionResult> GetTables()
    {
        try
        {
            var tables = new Dictionary<string, int>
            {
                ["users"] = await _context.Users.CountAsync(),
                ["pets"] = await _context.Pets.CountAsync(),
                ["posts"] = await _context.Posts.CountAsync(),
                ["user_tags"] = await _context.UserTags.CountAsync(),
                ["pet_vaccines"] = await _context.PetVaccines.CountAsync(),
                ["comments"] = await _context.Comments.CountAsync(),
                ["likes"] = await _context.Likes.CountAsync(),
                ["reports"] = await _context.Reports.CountAsync(),
                ["pettags"] = await _context.Pettags.CountAsync(),
                ["system_configs"] = await _context.SystemConfigs.CountAsync()
            };

            return Success(tables, "表信息获取成功");
        }
        catch (Exception ex)
        {
            return Error("获取表信息失败", new List<string> { ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        var healthInfo = new
        {
            Status = "Healthy",
            Service = "PetVerse API",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow
        };
        
        return Success(healthInfo, "服务运行正常");
    }
}