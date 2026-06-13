using EventManagement.API.Data;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db) => _db = db;

    public async Task<(IEnumerable<Event> Items, int TotalCount)> GetPublishedPagedAsync(
        int? categoryId, int? venueId, string? search, int page, int pageSize)
    {
        var query = _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.Status == EventStatus.Published);

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        if (venueId.HasValue)
            query = query.Where(e => e.VenueId == venueId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Title.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Event?> GetByIdWithDetailsAsync(int id)
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<(IEnumerable<Event> Items, int TotalCount)> GetByOrganizerPagedAsync(
        int organizerId, int page, int pageSize)
    {
        var query = _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.OrganizerId == organizerId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(IEnumerable<Event> Items, int TotalCount)> GetAllPagedAsync(
        EventStatus? status, int? organizerId, int page, int pageSize)
    {
        var query = _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (organizerId.HasValue)
            query = query.Where(e => e.OrganizerId == organizerId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Event> CreateAsync(Event evt)
    {
        _db.Events.Add(evt);
        await _db.SaveChangesAsync();
        return evt;
    }

    public async Task UpdateAsync(Event evt)
    {
        await _db.SaveChangesAsync();
    }

    public async Task<int> CountTotalAsync()
        => await _db.Events.CountAsync();

    public async Task<int> CountByStatusAsync(EventStatus status)
        => await _db.Events.CountAsync(e => e.Status == status);
}