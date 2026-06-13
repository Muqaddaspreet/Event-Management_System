using EventManagement.API.Data;
using EventManagement.API.Entities;
using EventManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.API.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _db;

    public VenueRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
        => await _db.Venues.OrderBy(v => v.Name).ToListAsync();

    public async Task<Venue?> GetByIdAsync(int id)
        => await _db.Venues.FindAsync(id);

    public async Task<bool> HasEventsAsync(int venueId)
        => await _db.Events.AnyAsync(e => e.VenueId == venueId);

    public async Task<Venue> CreateAsync(Venue venue)
    {
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();
        return venue;
    }

    public async Task UpdateAsync(Venue venue)
    {
        _db.Venues.Update(venue);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Venue venue)
    {
        _db.Venues.Remove(venue);
        await _db.SaveChangesAsync();
    }
}