using EventManagement.API.Data;
using EventManagement.API.Entities;
using EventManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await _db.Categories.OrderBy(c => c.Name).ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _db.Categories.FindAsync(id);

    public async Task<bool> NameExistsAsync(string name)
        => await _db.Categories.AnyAsync(c => c.Name == name);

    public async Task<bool> NameExistsForOtherAsync(string name, int excludeId)
        => await _db.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId);

    public async Task<bool> HasEventsAsync(int categoryId)
        => await _db.Events.AnyAsync(e => e.CategoryId == categoryId);

    public async Task<Category> CreateAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}