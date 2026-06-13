using EventManagement.API.Entities;

namespace EventManagement.API.Repositories.Interfaces;

public interface IVenueRepository
{
    Task<IEnumerable<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(int id);
    Task<bool> HasEventsAsync(int venueId);
    Task<Venue> CreateAsync(Venue venue);
    Task UpdateAsync(Venue venue);
    Task DeleteAsync(Venue venue);
}