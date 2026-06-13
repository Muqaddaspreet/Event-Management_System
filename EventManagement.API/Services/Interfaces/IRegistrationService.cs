using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Registrations;

namespace EventManagement.API.Services.Interfaces;

public interface IRegistrationService
{
    Task<IEnumerable<RegistrationResponse>> GetMineAsync(int userId);
    Task<ServiceResult<RegistrationResponse>> RegisterAsync(CreateRegistrationRequest request, int attendeeId);
    Task<ServiceResult<bool>> CancelAsync(int id, int callerId);
}