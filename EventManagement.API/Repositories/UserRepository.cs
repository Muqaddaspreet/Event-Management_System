using EventManagement.API.Data;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id)
        => await _db.Users.FindAsync(id);

    public async Task<bool> EmailExistsAsync(string email)
        => await _db.Users.AnyAsync(u => u.Email == email);

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetAllPagedAsync(
        UserRole? role, string? search, int page, int pageSize)
    {
        var query = _db.Users.AsQueryable();

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term)
                                  || u.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}