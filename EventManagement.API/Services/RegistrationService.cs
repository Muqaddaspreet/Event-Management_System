using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Registrations;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services.Interfaces;

namespace EventManagement.API.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IRegistrationRepository _regRepo;
    private readonly IEventRepository _eventRepo;

    public RegistrationService(IRegistrationRepository regRepo, IEventRepository eventRepo)
    {
        _regRepo   = regRepo;
        _eventRepo = eventRepo;
    }

    public async Task<IEnumerable<RegistrationResponse>> GetMineAsync(int userId)
    {
        var registrations = await _regRepo.GetByUserIdAsync(userId);
        return registrations.Select(MapToResponse);
    }

    public async Task<ServiceResult<RegistrationResponse>> RegisterAsync(
        CreateRegistrationRequest request, int attendeeId)
    {
        var evt = await _eventRepo.GetByIdWithDetailsAsync(request.EventId);
        if (evt is null)
            return ServiceResult<RegistrationResponse>.NotFound("Event not found.");

        if (evt.Status != EventStatus.Published)
            return ServiceResult<RegistrationResponse>.UnprocessableEntity(
                "Registration is only allowed for published events.");

        var existing = await _regRepo.FindByEventAndUserAsync(request.EventId, attendeeId);
        if (existing?.Status == RegistrationStatus.Registered)
            return ServiceResult<RegistrationResponse>.Conflict(
                "You are already registered for this event.");

        var activeCount = await _regRepo.CountActiveByEventIdAsync(request.EventId);
        if (activeCount >= evt.Capacity)
            return ServiceResult<RegistrationResponse>.UnprocessableEntity(
                "This event has reached its maximum capacity.");

        if (existing is not null)
        {
            // Reactivate cancelled registration
            existing.Status    = RegistrationStatus.Registered;
            existing.UpdatedAt = DateTime.UtcNow;
            await _regRepo.UpdateAsync(existing);

            var reloaded = await _regRepo.GetByIdWithDetailsAsync(existing.Id);
            if (reloaded is null)
                return ServiceResult<RegistrationResponse>.Failure("Registration could not be loaded.");
            return ServiceResult<RegistrationResponse>.Ok(MapToResponse(reloaded));
        }

        var registration = new EventRegistration
        {
            EventId      = request.EventId,
            UserId       = attendeeId,
            Status       = RegistrationStatus.Registered,
            RegisteredAt = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        await _regRepo.CreateAsync(registration);
        var created = await _regRepo.GetByIdWithDetailsAsync(registration.Id);
        if (created is null)
            return ServiceResult<RegistrationResponse>.Failure("Registration could not be loaded.");

        return ServiceResult<RegistrationResponse>.Ok(MapToResponse(created));
    }

    public async Task<ServiceResult<bool>> CancelAsync(int id, int callerId)
    {
        var reg = await _regRepo.GetByIdWithDetailsAsync(id);
        if (reg is null)
            return ServiceResult<bool>.NotFound("Registration not found.");

        if (reg.UserId != callerId)
            return ServiceResult<bool>.Forbidden(
                "You are not authorized to cancel this registration.");

        if (reg.Status == RegistrationStatus.Cancelled)
            return ServiceResult<bool>.Conflict("Registration is already cancelled.");

        reg.Status    = RegistrationStatus.Cancelled;
        reg.UpdatedAt = DateTime.UtcNow;
        await _regRepo.UpdateAsync(reg);
        return ServiceResult<bool>.Ok(true);
    }

    private static RegistrationResponse MapToResponse(EventRegistration r) => new()
    {
        Id             = r.Id,
        EventId        = r.EventId,
        EventTitle     = r.Event.Title,
        EventStartTime = r.Event.StartTime,
        UserId         = r.UserId,
        UserFullName   = r.User.FullName,
        Status         = r.Status.ToString(),
        RegisteredAt   = r.RegisteredAt,
        UpdatedAt      = r.UpdatedAt
    };
}