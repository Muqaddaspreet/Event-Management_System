using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Registrations;
using EventManagement.API.DTOs.Users;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services.Interfaces;

namespace EventManagement.API.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepo;
    private readonly IRegistrationRepository _regRepo;

    public AdminService(IUserRepository userRepo, IRegistrationRepository regRepo)
    {
        _userRepo = userRepo;
        _regRepo  = regRepo;
    }

    public async Task<PagedResult<UserResponse>> GetUsersPagedAsync(
        UserRole? role, string? search, int page, int pageSize)
    {
        page     = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 50 ? 50 : pageSize;

        var (items, total) = await _userRepo.GetAllPagedAsync(role, search, page, pageSize);
        return new PagedResult<UserResponse>
        {
            Items      = items.Select(MapUserToResponse),
            Page       = page,
            PageSize   = pageSize,
            TotalCount = total
        };
    }

    public async Task<ServiceResult<UserResponse>> GetUserByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null)
            return ServiceResult<UserResponse>.NotFound("User not found.");
        return ServiceResult<UserResponse>.Ok(MapUserToResponse(user));
    }

    public async Task<PagedResult<AdminRegistrationResponse>> GetRegistrationsPagedAsync(
        int? eventId, int? userId, RegistrationStatus? status, int page, int pageSize)
    {
        page     = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 50 ? 50 : pageSize;

        var (items, total) = await _regRepo.GetAllPagedAsync(eventId, userId, status, page, pageSize);
        return new PagedResult<AdminRegistrationResponse>
        {
            Items      = items.Select(MapRegistrationToResponse),
            Page       = page,
            PageSize   = pageSize,
            TotalCount = total
        };
    }

    private static UserResponse MapUserToResponse(User u) => new()
    {
        Id        = u.Id,
        FullName  = u.FullName,
        Email     = u.Email,
        Role      = u.Role.ToString(),
        CreatedAt = u.CreatedAt
    };

    private static AdminRegistrationResponse MapRegistrationToResponse(EventRegistration r) => new()
    {
        Id           = r.Id,
        EventId      = r.EventId,
        EventTitle   = r.Event.Title,
        UserId       = r.UserId,
        UserFullName = r.User.FullName,
        UserEmail    = r.User.Email,
        Status       = r.Status.ToString(),
        RegisteredAt = r.RegisteredAt,
        UpdatedAt    = r.UpdatedAt
    };
}