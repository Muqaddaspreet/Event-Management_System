using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Venues;

namespace EventManagement.API.Services.Interfaces;

public interface IVenueService
{
    Task<IEnumerable<VenueResponse>> GetAllAsync();
    Task<ServiceResult<VenueResponse>> CreateAsync(CreateVenueRequest request);
    Task<ServiceResult<VenueResponse>> UpdateAsync(int id, UpdateVenueRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}