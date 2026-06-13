using EventManagement.API.DTOs.Admin;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services.Interfaces;

namespace EventManagement.API.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUserRepository _userRepo;
    private readonly IEventRepository _eventRepo;
    private readonly IRegistrationRepository _regRepo;

    public AdminDashboardService(
        IUserRepository userRepo,
        IEventRepository eventRepo,
        IRegistrationRepository regRepo)
    {
        _userRepo  = userRepo;
        _eventRepo = eventRepo;
        _regRepo   = regRepo;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync()
    {
        return new AdminDashboardResponse
        {
            TotalUsers         = await _userRepo.CountTotalAsync(),
            TotalEvents        = await _eventRepo.CountTotalAsync(),
            PendingEvents      = await _eventRepo.CountByStatusAsync(EventStatus.PendingApproval),
            PublishedEvents    = await _eventRepo.CountByStatusAsync(EventStatus.Published),
            TotalRegistrations = await _regRepo.CountTotalAsync()
        };
    }
}