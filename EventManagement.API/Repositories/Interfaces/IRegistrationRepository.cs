using EventManagement.API.Entities;

namespace EventManagement.API.Repositories.Interfaces;

public interface IRegistrationRepository
{
    Task<IEnumerable<EventRegistration>> GetByUserIdAsync(int userId);
    Task<EventRegistration?> GetByIdWithDetailsAsync(int id);
    Task<EventRegistration?> FindByEventAndUserAsync(int eventId, int userId);
    Task<int> CountActiveByEventIdAsync(int eventId);
    Task<EventRegistration> CreateAsync(EventRegistration registration);
    Task UpdateAsync(EventRegistration registration);
}