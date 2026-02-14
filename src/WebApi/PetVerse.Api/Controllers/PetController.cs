using Microsoft.AspNetCore.Mvc;
using PetVerse.Core.DTOs;
using PetVerse.Core.DTOs.Pet;
using PetVerse.Core.Interfaces;

namespace PetVerse.Api.Controllers;

/// <summary>
/// 宠物管理控制器
/// </summary>
public class PetController : BaseController
{
    private readonly IPetService _petService;
    private readonly ILogger<PetController> _logger;

    public PetController(IPetService petService, ILogger<PetController> logger)
    {
        _petService = petService;
        _logger = logger;
    }

    /// <summary>
    /// 获取宠物列表（分页）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPets([FromQuery] PetQueryParams queryParams)
    {
        try
        {
            var (pets, totalCount) = await _petService.GetAllPetsAsync(queryParams);
            
            var paginationInfo = new PaginationInfo
            {
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / queryParams.PageSize)
            };

            var result = new
            {
                Pets = pets,
                Pagination = paginationInfo
            };

            return Success(result, "获取宠物列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取宠物列表时发生错误");
            return InternalError("获取宠物列表失败");
        }
    }

    /// <summary>
    /// 根据ID获取宠物详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPet(ulong id)
    {
        try
        {
            var pet = await _petService.GetPetByIdAsync(id);
            
            if (pet == null)
                return NotFound("宠物不存在");

            return Success(pet, "获取宠物信息成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取宠物信息时发生错误，ID: {PetId}", id);
            return InternalError("获取宠物信息失败");
        }
    }

    /// <summary>
    /// 根据用户ID获取宠物列表
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetPetsByUser(ulong userId)
    {
        try
        {
            var pets = await _petService.GetPetsByUserIdAsync(userId);
            return Success(pets, "获取用户宠物列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户宠物列表时发生错误，UserID: {UserId}", userId);
            return InternalError("获取用户宠物列表失败");
        }
    }

    /// <summary>
    /// 创建宠物
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePet([FromBody] CreatePetRequest request)
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

            var pet = await _petService.CreatePetAsync(request);
            return Success(pet, "宠物创建成功", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建宠物时发生错误");
            return InternalError("创建宠物失败");
        }
    }

    /// <summary>
    /// 更新宠物信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(ulong id, [FromBody] UpdatePetRequest request)
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

            var pet = await _petService.UpdatePetAsync(id, request);
            
            if (pet == null)
                return NotFound("宠物不存在");

            return Success(pet, "宠物信息更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新宠物信息时发生错误，ID: {PetId}", id);
            return InternalError("更新宠物信息失败");
        }
    }

    /// <summary>
    /// 删除宠物
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(ulong id)
    {
        try
        {
            var result = await _petService.DeletePetAsync(id);
            
            if (!result)
                return NotFound("宠物不存在");

            return SuccessNoData("宠物删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除宠物时发生错误，ID: {PetId}", id);
            return InternalError("删除宠物失败");
        }
    }

    /// <summary>
    /// 检查宠物是否存在
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<IActionResult> CheckPetExists(ulong id)
    {
        try
        {
            var exists = await _petService.PetExistsAsync(id);
            var result = new { Exists = exists };
            return Success(result, "检查完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查宠物存在性时发生错误，ID: {PetId}", id);
            return InternalError("检查宠物失败");
        }
    }

    /// <summary>
    /// 验证宠物归属关系
    /// </summary>
    [HttpGet("{petId}/belongs-to/{userId}")]
    public async Task<IActionResult> CheckPetOwnership(ulong petId, ulong userId)
    {
        try
        {
            var belongs = await _petService.PetBelongsToUserAsync(petId, userId);
            var result = new { BelongsToUser = belongs };
            return Success(result, "验证完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证宠物归属关系时发生错误，PetId: {PetId}, UserId: {UserId}", petId, userId);
            return InternalError("验证宠物归属关系失败");
        }
    }
}