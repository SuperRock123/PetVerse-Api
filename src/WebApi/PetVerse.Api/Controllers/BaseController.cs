using Microsoft.AspNetCore.Mvc;
using PetVerse.Core.DTOs;

namespace PetVerse.Api.Controllers;

/// <summary>
/// 控制器基类，提供统一的响应方法
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// 返回成功响应
    /// </summary>
    protected IActionResult Success<T>(T data, string message = "操作成功", int statusCode = 200)
    {
        return Ok(ApiResponse<T>.CreateSuccess(data, message, statusCode));
    }

    /// <summary>
    /// 返回成功响应（无数据）
    /// </summary>
    protected IActionResult SuccessNoData(string message = "操作成功", int statusCode = 200)
    {
        return Ok(ApiResponse<object>.CreateSuccess(null, message, statusCode));
    }

    /// <summary>
    /// 返回错误响应
    /// </summary>
    protected IActionResult Error(string message, List<string>? errors = null, int statusCode = 400)
    {
        return BadRequest(ApiResponse<object>.CreateError(message, errors, statusCode));
    }

    /// <summary>
    /// 返回未找到响应
    /// </summary>
    protected IActionResult NotFound(string message = "资源未找到")
    {
        return NotFound(ApiResponse<object>.CreateError(message, null, 404));
    }

    /// <summary>
    /// 返回服务器内部错误响应
    /// </summary>
    protected IActionResult InternalError(string message = "服务器内部错误")
    {
        return StatusCode(500, ApiResponse<object>.CreateError(message, null, 500));
    }
}