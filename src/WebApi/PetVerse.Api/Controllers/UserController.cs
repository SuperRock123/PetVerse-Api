using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetVerse.Core.DTOs;
using PetVerse.Core.DTOs.User;
using PetVerse.Core.Interfaces;

namespace PetVerse.Api.Controllers;

/// <summary>
/// 用户管理控制器
/// </summary>
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户列表（分页）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryParams queryParams)
    {
        try
        {
            var (users, totalCount) = await _userService.GetAllUsersAsync(queryParams);
            
            var paginationInfo = new PaginationInfo
            {
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / queryParams.PageSize)
            };

            var result = new
            {
                Users = users,
                Pagination = paginationInfo
            };

            return Success(result, "获取用户列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户列表时发生错误");
            return InternalError("获取用户列表失败");
        }
    }

    /// <summary>
    /// 根据ID获取用户详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(ulong id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
                return NotFound("用户不存在");

            return Success(user, "获取用户信息成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户信息时发生错误，ID: {UserId}", id);
            return InternalError("获取用户信息失败");
        }
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var user = await _userService.CreateUserAsync(request);
            return Success(user, "用户创建成功", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户时发生错误");
            return InternalError("创建用户失败");
        }
    }

    /// <summary>
    /// 用户注册（公开接口，无需认证）
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var user = await _userService.CreateUserAsync(request);
            return Success(user, "用户注册成功", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户注册时发生错误");
            return InternalError("用户注册失败");
        }
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(ulong id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var user = await _userService.UpdateUserAsync(id, request);
            
            if (user == null)
                return NotFound("用户不存在");

            return Success(user, "用户信息更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用户信息时发生错误，ID: {UserId}", id);
            return InternalError("更新用户信息失败");
        }
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(ulong id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            
            if (!result)
                return NotFound("用户不存在");

            return SuccessNoData("用户删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除用户时发生错误，ID: {UserId}", id);
            return InternalError("删除用户失败");
        }
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Error("数据验证失败", errors);
            }

            var loginResult = await _userService.LoginAsync(request);
            
            if (loginResult == null)
                return Error("用户名或密码错误", null, 401);

            return Success(loginResult, "登录成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户登录时发生错误");
            return InternalError("登录失败");
        }
    }

    /// <summary>
    /// 检查用户名是否可用
    /// </summary>
    [HttpGet("check-username/{username}")]
    public async Task<IActionResult> CheckUsername(string username)
    {
        try
        {
            var exists = await _userService.UsernameExistsAsync(username);
            var result = new { Available = !exists };
            return Success(result, "检查完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户名时发生错误，Username: {Username}", username);
            return InternalError("检查用户名失败");
        }
    }

    /// <summary>
    /// 检查邮箱是否可用
    /// </summary>
    [HttpGet("check-email/{email}")]
    public async Task<IActionResult> CheckEmail(string email)
    {
        try
        {
            var exists = await _userService.EmailExistsAsync(email);
            var result = new { Available = !exists };
            return Success(result, "检查完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查邮箱时发生错误，Email: {Email}", email);
            return InternalError("检查邮箱失败");
        }
    }
}