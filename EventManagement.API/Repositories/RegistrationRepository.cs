using EventManagement.API.Data;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Repositories;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly AppDbContext _db;

    public RegistrationRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<EventRegistration>> GetByUserIdAsync(int userId)
    {
        return await _db.EventRegistrations
            .Include(r => r.Event)
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();
    }

    public async Task<EventRegistration?> GetByIdWithDetailsAsync(int id)
    {
        return await _db.EventRegistrations
            .Include(r => r.Event)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<EventRegistration?> FindByEventAndUserAsync(int eventId, int userId)
    {
        return await _db.EventRegistrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
    }

    public async Task<int> CountActiveByEventIdAsync(int eventId)
    {
        return await _db.EventRegistrations
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Registered);
    }

    public async Task<EventRegistration> CreateAsync(EventRegistration registration)
    {
        _db.EventRegistrations.Add(registration);
        await _db.SaveChangesAsync();
        return registration;
    }

    public async Task UpdateAsync(EventRegistration registration)
    {
        await _db.SaveChangesAsync();
    }
}