using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Registrations;
using EventManagement.API.DTOs.Users;
using EventManagement.API.Enums;

namespace EventManagement.API.Services.Interfaces;

public interface IAdminService
{
    Task<PagedResult<UserResponse>> GetUsersPagedAsync(
        UserRole? role, string? search, int page, int pageSize);
    Task<ServiceResult<UserResponse>> GetUserByIdAsync(int id);
    Task<PagedResult<AdminRegistrationResponse>> GetRegistrationsPagedAsync(
        int? eventId, int? userId, RegistrationStatus? status, int page, int pageSize);
}