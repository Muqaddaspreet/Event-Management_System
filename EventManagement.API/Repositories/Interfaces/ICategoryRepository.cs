using EventManagement.API.Entities;

namespace EventManagement.API.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name);
    Task<bool> NameExistsForOtherAsync(string name, int excludeId);
    Task<bool> HasEventsAsync(int categoryId);
    Task<Category> CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}