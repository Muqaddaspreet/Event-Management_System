using EventManagement.API.Entities;
using EventManagement.API.Enums;

namespace EventManagement.API.Repositories.Interfaces;

public interface IEventRepository
{
    Task<(IEnumerable<Event> Items, int TotalCount)> GetPublishedPagedAsync(
        int? categoryId, int? venueId, string? search, int page, int pageSize);

    Task<Event?> GetByIdWithDetailsAsync(int id);

    Task<(IEnumerable<Event> Items, int TotalCount)> GetByOrganizerPagedAsync(
        int organizerId, int page, int pageSize);

    Task<(IEnumerable<Event> Items, int TotalCount)> GetAllPagedAsync(
        EventStatus? status, int? organizerId, int page, int pageSize);

    Task<Event> CreateAsync(Event evt);

    Task UpdateAsync(Event evt);
}