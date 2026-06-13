using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Venues;
using EventManagement.API.Entities;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services.Interfaces;

namespace EventManagement.API.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _repo;

    public VenueService(IVenueRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<VenueResponse>> GetAllAsync()
    {
        var venues = await _repo.GetAllAsync();
        return venues.Select(MapToResponse);
    }

    public async Task<ServiceResult<VenueResponse>> CreateAsync(CreateVenueRequest request)
    {
        if (request.Capacity <= 0)
            return ServiceResult<VenueResponse>.Failure("Venue capacity must be greater than 0.");

        var venue = new Venue
        {
            Name     = request.Name,
            Address  = request.Address,
            City     = request.City,
            Capacity = request.Capacity
        };

        await _repo.CreateAsync(venue);

        return ServiceResult<VenueResponse>.Ok(MapToResponse(venue));
    }

    public async Task<ServiceResult<VenueResponse>> UpdateAsync(int id, UpdateVenueRequest request)
    {
        var venue = await _repo.GetByIdAsync(id);
        if (venue is null)
            return ServiceResult<VenueResponse>.NotFound($"Venue with id {id} not found.");

        if (request.Capacity <= 0)
            return ServiceResult<VenueResponse>.Failure("Venue capacity must be greater than 0.");

        venue.Name     = request.Name;
        venue.Address  = request.Address;
        venue.City     = request.City;
        venue.Capacity = request.Capacity;

        await _repo.UpdateAsync(venue);

        return ServiceResult<VenueResponse>.Ok(MapToResponse(venue));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var venue = await _repo.GetByIdAsync(id);
        if (venue is null)
            return ServiceResult<bool>.NotFound($"Venue with id {id} not found.");

        if (await _repo.HasEventsAsync(id))
            return ServiceResult<bool>.Conflict("Cannot delete venue: one or more events reference it.");

        await _repo.DeleteAsync(venue);
        return ServiceResult<bool>.Ok(true);
    }

    private static VenueResponse MapToResponse(Venue v) => new()
    {
        Id       = v.Id,
        Name     = v.Name,
        Address  = v.Address,
        City     = v.City,
        Capacity = v.Capacity
    };
}