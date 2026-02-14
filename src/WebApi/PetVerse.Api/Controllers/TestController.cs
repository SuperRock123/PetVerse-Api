using Microsoft.AspNetCore.Mvc;

namespace PetVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("simple")]
    public IActionResult SimpleTest()
    {
        var data = new { 
            Message = "这是一个简单测试", 
            Timestamp = DateTime.UtcNow 
        };
        
        // 直接返回原始数据，让全局过滤器自动包装
        return Ok(data);
    }

    [HttpGet("error")]
    public IActionResult ErrorTest()
    {
        return BadRequest("这是一个错误测试");
    }

    [HttpPost("validation")]
    public IActionResult ValidationTest([FromBody] TestModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        return Ok(new { Result = "验证通过" });
    }
}

public class TestModel
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}