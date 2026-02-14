using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetVerse.Core.DTOs.Pet;
using PetVerse.Core.Entities;
using PetVerse.Core.Exceptions;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Infrastructure.Services;

/// <summary>
/// 宠物服务实现
/// </summary>
public class PetService : IPetService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PetService> _logger;

    public PetService(ApplicationDbContext context, ILogger<PetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<PetResponse> Pets, int TotalCount)> GetAllPetsAsync(PetQueryParams queryParams)
    {
        try
        {
            var query = _context.Pets
                .Include(p => p.User)
                .AsQueryable();

            // 应用查询条件
            if (queryParams.UserId.HasValue)
            {
                query = query.Where(p => p.UserId == queryParams.UserId.Value);
            }

            if (!string.IsNullOrEmpty(queryParams.Keyword))
            {
                query = query.Where(p => p.Name.Contains(queryParams.Keyword) || 
                                       p.Breed.Contains(queryParams.Keyword));
            }

            if (!string.IsNullOrEmpty(queryParams.Breed))
            {
                query = query.Where(p => p.Breed == queryParams.Breed);
            }

            if (queryParams.Gender.HasValue)
            {
                query = query.Where(p => p.Gender == queryParams.Gender.Value);
            }

            // 获取总数
            var totalCount = await query.CountAsync();

            // 应用分页
            var pets = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            var petResponses = pets.Select(MapToPetResponse).ToList();

            return (petResponses, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取宠物列表时发生错误");
            throw new ServiceException("获取宠物列表失败", ex);
        }
    }

    public async Task<PetDetailResponse?> GetPetByIdAsync(ulong id)
    {
        try
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .Include(p => p.Posts)
                .Include(p => p.PetVaccines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null)
                return null;

            return MapToPetDetailResponse(pet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据ID获取宠物时发生错误，ID: {PetId}", id);
            throw new ServiceException("获取宠物信息失败", ex);
        }
    }

    public async Task<List<PetResponse>> GetPetsByUserIdAsync(ulong userId)
    {
        try
        {
            var pets = await _context.Pets
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return pets.Select(MapToPetResponse).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据用户ID获取宠物列表时发生错误，UserID: {UserId}", userId);
            throw new ServiceException("获取宠物列表失败", ex);
        }
    }

    public async Task<PetResponse> CreatePetAsync(CreatePetRequest request)
    {
        try
        {
            // 验证用户是否存在
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
            if (!userExists)
                throw new NotFoundException($"用户ID {request.UserId} 不存在");

            var pet = new Pet
            {
                UserId = request.UserId,
                Name = request.Name,
                Breed = request.Breed,
                Gender = request.Gender,
                Birthday = request.Birthday,
                WeightKg = request.WeightKg,
                HealthStatus = request.HealthStatus,
                AvatarUrl = request.AvatarUrl,
                PettagId = request.PettagId
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            // 重新加载包含用户信息的数据
            var createdPet = await _context.Pets
                .Include(p => p.User)
                .FirstAsync(p => p.Id == pet.Id);

            return MapToPetResponse(createdPet);
        }
        catch (NotFoundException)
        {
            throw; // 重新抛出未找到异常
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建宠物时发生错误");
            throw new ServiceException("创建宠物失败", ex);
        }
    }

    public async Task<PetResponse?> UpdatePetAsync(ulong id, UpdatePetRequest request)
    {
        try
        {
            var pet = await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null)
                throw new NotFoundException($"宠物ID {id} 不存在");

            // 更新宠物信息
            if (request.Name != null) pet.Name = request.Name;
            if (request.Breed != null) pet.Breed = request.Breed;
            if (request.Gender.HasValue) pet.Gender = request.Gender.Value;
            if (request.Birthday.HasValue) pet.Birthday = request.Birthday.Value;
            if (request.WeightKg.HasValue) pet.WeightKg = request.WeightKg.Value;
            if (request.HealthStatus != null) pet.HealthStatus = request.HealthStatus;
            if (request.AvatarUrl != null) pet.AvatarUrl = request.AvatarUrl;
            if (request.PettagId != null) pet.PettagId = request.PettagId;

            pet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToPetResponse(pet);
        }
        catch (NotFoundException)
        {
            throw; // 重新抛出未找到异常
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新宠物时发生错误，ID: {PetId}", id);
            throw new ServiceException("更新宠物失败", ex);
        }
    }

    public async Task<bool> DeletePetAsync(ulong id)
    {
        try
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
                return false;

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除宠物时发生错误，ID: {PetId}", id);
            throw new ServiceException("删除宠物失败", ex);
        }
    }

    public async Task<bool> PetExistsAsync(ulong id)
    {
        try
        {
            return await _context.Pets.AnyAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证宠物存在性时发生错误，ID: {PetId}", id);
            throw new ServiceException("验证宠物失败", ex);
        }
    }

    public async Task<bool> PetBelongsToUserAsync(ulong petId, ulong userId)
    {
        try
        {
            return await _context.Pets.AnyAsync(p => p.Id == petId && p.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证宠物归属关系时发生错误，PetId: {PetId}, UserId: {UserId}", petId, userId);
            throw new ServiceException("验证宠物归属关系失败", ex);
        }
    }

    #region 私有方法

    private PetResponse MapToPetResponse(Pet pet)
    {
        return new PetResponse
        {
            Id = pet.Id,
            UserId = pet.UserId,
            Name = pet.Name,
            Breed = pet.Breed,
            Gender = pet.Gender,
            Birthday = pet.Birthday,
            WeightKg = pet.WeightKg,
            HealthStatus = pet.HealthStatus,
            AvatarUrl = pet.AvatarUrl,
            PettagId = pet.PettagId,
            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt,
            UserName = pet.User?.Username ?? "",
            UserAvatar = pet.User?.AvatarUrl
        };
    }

    private PetDetailResponse MapToPetDetailResponse(Pet pet)
    {
        var response = new PetDetailResponse
        {
            Id = pet.Id,
            UserId = pet.UserId,
            Name = pet.Name,
            Breed = pet.Breed,
            Gender = pet.Gender,
            Birthday = pet.Birthday,
            WeightKg = pet.WeightKg,
            HealthStatus = pet.HealthStatus,
            AvatarUrl = pet.AvatarUrl,
            PettagId = pet.PettagId,
            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt,
            UserName = pet.User?.Username ?? "",
            UserAvatar = pet.User?.AvatarUrl,
            PostsCount = pet.Posts.Count,
            VaccinesCount = pet.PetVaccines.Count,
            Vaccines = pet.PetVaccines.Select(v => new VaccineInfo
            {
                VaccineName = v.VaccineName,
                VaccinationDate = v.VaccinateDate,
                NextDueDate = v.NextDate,
                Notes = v.Remark
            }).ToList()
        };

        return response;
    }

    #endregion
}