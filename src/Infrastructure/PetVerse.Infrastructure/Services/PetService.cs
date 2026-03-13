using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetVerse.Core.DTOs.Pet;
using PetVerse.Core.Entities;
using PetVerse.Core.Exceptions;
using PetVerse.Core.Interfaces;
using PetVerse.Infrastructure.Data;

namespace PetVerse.Infrastructure.Services
{
    public class PetService(ApplicationDbContext context, IStorageService storageService, ILogger<PetService> logger) : IPetService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IStorageService _storageService = storageService;
        private readonly ILogger<PetService> _logger = logger;

        public async Task<(List<PetResponse> Pets, int TotalCount)> GetAllPetsAsync(PetQueryParams queryParams)
        {
            try
            {
                IQueryable<Pet> query = _context.Pets
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
                int totalCount = await query.CountAsync();

                // 应用分页
                List<Pet> pets = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((queryParams.Page - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync();

                List<PetResponse> petResponses = [];
                foreach (Pet? pet in pets)
                {
                    petResponses.Add(await MapToPetResponseAsync(pet));
                }

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
                Pet? pet = await _context.Pets
                    .Include(p => p.User)
                    .Include(p => p.Posts)
                    .Include(p => p.PetVaccines)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return pet == null ? null : await MapToPetDetailResponseAsync(pet);
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
                List<Pet> pets = await _context.Pets
                    .Include(p => p.User)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                List<PetResponse> petResponses = [];
                foreach (Pet? pet in pets)
                {
                    petResponses.Add(await MapToPetResponseAsync(pet));
                }

                return petResponses;
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
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.Status == 1);
                if (!userExists)
                {
                    throw new NotFoundException($"用户ID {request.UserId} 不存在");
                }

                Pet pet = new()
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

                _ = _context.Pets.Add(pet);
                _ = await _context.SaveChangesAsync();

                // 重新加载包含用户信息的数据
                Pet createdPet = await _context.Pets
                    .Include(p => p.User)
                    .FirstAsync(p => p.Id == pet.Id);

                return await MapToPetResponseAsync(createdPet);
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
                Pet pet = await _context.Pets
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id) ?? throw new NotFoundException($"宠物ID {id} 不存在");

                // 更新宠物信息
                if (request.Name != null)
                {
                    pet.Name = request.Name;
                }

                if (request.Breed != null)
                {
                    pet.Breed = request.Breed;
                }

                if (request.Gender.HasValue)
                {
                    pet.Gender = request.Gender.Value;
                }

                if (request.Birthday.HasValue)
                {
                    pet.Birthday = request.Birthday.Value;
                }

                if (request.WeightKg.HasValue)
                {
                    pet.WeightKg = request.WeightKg.Value;
                }

                if (request.HealthStatus != null)
                {
                    pet.HealthStatus = request.HealthStatus;
                }

                if (request.AvatarUrl != null)
                {
                    pet.AvatarUrl = request.AvatarUrl;
                }

                if (request.PettagId != null)
                {
                    pet.PettagId = request.PettagId;
                }

                pet.UpdatedAt = DateTime.UtcNow;

                _ = await _context.SaveChangesAsync();
                return await MapToPetResponseAsync(pet);
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
                Pet? pet = await _context.Pets.FindAsync(id);
                if (pet == null)
                {
                    return false;
                }

                _ = _context.Pets.Remove(pet);
                _ = await _context.SaveChangesAsync();
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

        private async Task<PetResponse> MapToPetResponseAsync(Pet pet)
        {
            string? avatarUrl = pet.AvatarUrl;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    avatarUrl = await _storageService.GetFileUrlAsync(avatarUrl, 10);
                }
                catch
                {
                    avatarUrl = null;
                }
            }

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
                AvatarUrl = avatarUrl,
                PettagId = pet.PettagId,
                CreatedAt = pet.CreatedAt,
                UpdatedAt = pet.UpdatedAt,
                UserName = pet.User?.Username ?? "",
                UserAvatar = pet.User?.AvatarUrl
            };
        }

        private async Task<PetDetailResponse> MapToPetDetailResponseAsync(Pet pet)
        {
            string? avatarUrl = pet.AvatarUrl;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    avatarUrl = await _storageService.GetFileUrlAsync(avatarUrl, 10);
                }
                catch
                {
                    avatarUrl = null;
                }
            }

            PetDetailResponse response = new()
            {
                Id = pet.Id,
                UserId = pet.UserId,
                Name = pet.Name,
                Breed = pet.Breed,
                Gender = pet.Gender,
                Birthday = pet.Birthday,
                WeightKg = pet.WeightKg,
                HealthStatus = pet.HealthStatus,
                AvatarUrl = avatarUrl,
                PettagId = pet.PettagId,
                CreatedAt = pet.CreatedAt,
                UpdatedAt = pet.UpdatedAt,
                UserName = pet.User?.Username ?? "",
                UserAvatar = pet.User?.AvatarUrl,
                PostsCount = pet.Posts.Count,
                VaccinesCount = pet.PetVaccines.Count,
                Vaccines = [.. pet.PetVaccines.Select(static v => new VaccineInfo
                {
                    VaccineName = v.VaccineName,
                    VaccinationDate = v.VaccinateDate,
                    NextDueDate = v.NextDate,
                    Notes = v.Remark
                })]
            };

            return response;
        }

        #endregion
    }
}