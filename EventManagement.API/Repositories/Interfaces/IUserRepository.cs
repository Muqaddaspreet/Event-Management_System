using EventManagement.API.Entities;
using EventManagement.API.Enums;

namespace EventManagement.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateAsync(User user);
    Task<(IEnumerable<User> Items, int TotalCount)> GetAllPagedAsync(
        UserRole? role, string? search, int page, int pageSize);
    Task<int> CountTotalAsync();
}