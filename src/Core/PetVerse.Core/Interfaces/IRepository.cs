namespace PetVerse.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<bool> ExistsAsync(object id);
}