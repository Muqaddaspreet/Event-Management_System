using EventManagement.API.Entities;
using EventManagement.API.Enums;

namespace EventManagement.API.Repositories.Interfaces;

public interface IRegistrationRepository
{
    Task<IEnumerable<EventRegistration>> GetByUserIdAsync(int userId);
    Task<EventRegistration?> GetByIdWithDetailsAsync(int id);
    Task<EventRegistration?> FindByEventAndUserAsync(int eventId, int userId);
    Task<int> CountActiveByEventIdAsync(int eventId);
    Task<EventRegistration> CreateAsync(EventRegistration registration);
    Task UpdateAsync(EventRegistration registration);
    Task<(IEnumerable<EventRegistration> Items, int TotalCount)> GetAllPagedAsync(
        int? eventId, int? userId, RegistrationStatus? status, int page, int pageSize);
}