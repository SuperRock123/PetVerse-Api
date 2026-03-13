using PetVerse.Core.DTOs.Pet;

namespace PetVerse.Core.Interfaces
{
    /// <summary>
    /// 宠物服务接口
    /// </summary>
    public interface IPetService
    {
        /// <summary>
        /// 获取所有宠物（分页）
        /// </summary>
        Task<(List<PetResponse> Pets, int TotalCount)> GetAllPetsAsync(PetQueryParams queryParams);

        /// <summary>
        /// 根据ID获取宠物
        /// </summary>
        Task<PetDetailResponse?> GetPetByIdAsync(ulong id);

        /// <summary>
        /// 根据用户ID获取宠物列表
        /// </summary>
        Task<List<PetResponse>> GetPetsByUserIdAsync(ulong userId);

        /// <summary>
        /// 创建宠物
        /// </summary>
        Task<PetResponse> CreatePetAsync(CreatePetRequest request);

        /// <summary>
        /// 更新宠物
        /// </summary>
        Task<PetResponse?> UpdatePetAsync(ulong id, UpdatePetRequest request);

        /// <summary>
        /// 删除宠物
        /// </summary>
        Task<bool> DeletePetAsync(ulong id);

        /// <summary>
        /// 验证宠物是否存在
        /// </summary>
        Task<bool> PetExistsAsync(ulong id);

        /// <summary>
        /// 验证宠物是否属于指定用户
        /// </summary>
        Task<bool> PetBelongsToUserAsync(ulong petId, ulong userId);
    }
}