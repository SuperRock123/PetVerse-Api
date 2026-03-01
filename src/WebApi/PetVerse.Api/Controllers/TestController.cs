using Microsoft.AspNetCore.Mvc;

namespace PetVerse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : BaseController
{
    [HttpGet("simple")]
    public IActionResult SimpleTest()
    {
        var data = new { 
            Message = "这是一个简单测试", 
            Timestamp = DateTime.UtcNow 
        };
        
        return Success(data, "测试成功");
    }

    [HttpGet("error")]
    public IActionResult ErrorTest()
    {
        return Error("这是一个错误测试");
    }

    [HttpPost("validation")]
    public IActionResult ValidationTest([FromBody] TestModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Error("数据验证失败", errors);
        }
        
        return Success(new { Result = "验证通过" }, "验证通过");
    }
}

public class TestModel
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}