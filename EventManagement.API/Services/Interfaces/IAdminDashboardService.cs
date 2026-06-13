using EventManagement.API.DTOs.Admin;

namespace EventManagement.API.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardResponse> GetDashboardAsync();
}