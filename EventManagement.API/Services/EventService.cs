using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Events;
using EventManagement.API.Entities;
using EventManagement.API.Enums;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services.Interfaces;

namespace EventManagement.API.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IVenueRepository _venueRepo;

    public EventService(
        IEventRepository eventRepo,
        ICategoryRepository categoryRepo,
        IVenueRepository venueRepo)
    {
        _eventRepo = eventRepo;
        _categoryRepo = categoryRepo;
        _venueRepo = venueRepo;
    }

    public async Task<PagedResult<EventSummaryResponse>> GetPublishedAsync(
        int? categoryId, int? venueId, string? search, int page, int pageSize)
    {
        page     = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 50 ? 50 : pageSize;

        var (items, total) = await _eventRepo.GetPublishedPagedAsync(
            categoryId, venueId, search, page, pageSize);

        return new PagedResult<EventSummaryResponse>
        {
            Items      = items.Select(MapToSummary),
            Page       = page,
            PageSize   = pageSize,
            TotalCount = total
        };
    }

    public async Task<ServiceResult<EventDetailResponse>> GetByIdAsync(
        int id, int? callerId, string? callerRole)
    {
        var evt = await _eventRepo.GetByIdWithDetailsAsync(id);
        if (evt is null)
            return ServiceResult<EventDetailResponse>.NotFound("Event not found.");

        bool isAdmin          = callerRole == "Admin";
        bool isOrganizerOwner = callerRole == "Organizer" && evt.OrganizerId == callerId;
        bool isPublished      = evt.Status == EventStatus.Published;

        if (!isAdmin && !isOrganizerOwner && !isPublished)
            return ServiceResult<EventDetailResponse>.NotFound("Event not found.");

        return ServiceResult<EventDetailResponse>.Ok(MapToDetail(evt));
    }

    public async Task<PagedResult<EventSummaryResponse>> GetByOrganizerAsync(
        int organizerId, int page, int pageSize)
    {
        page     = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 50 ? 50 : pageSize;

        var (items, total) = await _eventRepo.GetByOrganizerPagedAsync(
            organizerId, page, pageSize);

        return new PagedResult<EventSummaryResponse>
        {
            Items      = items.Select(MapToSummary),
            Page       = page,
            PageSize   = pageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResult<EventSummaryResponse>> GetAllForAdminAsync(
        EventStatus? status, int? organizerId, int page, int pageSize)
    {
        page     = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 50 ? 50 : pageSize;

        var (items, total) = await _eventRepo.GetAllPagedAsync(status, organizerId, page, pageSize);

        return new PagedResult<EventSummaryResponse>
        {
            Items      = items.Select(MapToSummary),
            Page       = page,
            PageSize   = pageSize,
            TotalCount = total
        };
    }

    public async Task<ServiceResult<EventDetailResponse>> ApproveAsync(int id)
    {
        var evt = await _eventRepo.GetByIdWithDetailsAsync(id);
        if (evt is null)
            return ServiceResult<EventDetailResponse>.NotFound("Event not found.");

        if (evt.Status != EventStatus.PendingApproval)
            return ServiceResult<EventDetailResponse>.Conflict(
                $"Event cannot be approved because its current status is {evt.Status}.");

        evt.Status    = EventStatus.Published;
        evt.UpdatedAt = DateTime.UtcNow;
        await _eventRepo.UpdateAsync(evt);
        return ServiceResult<EventDetailResponse>.Ok(MapToDetail(evt));
    }

    public async Task<ServiceResult<EventDetailResponse>> RejectAsync(int id, RejectEventRequest request)
    {
        var evt = await _eventRepo.GetByIdWithDetailsAsync(id);
        if (evt is null)
            return ServiceResult<EventDetailResponse>.NotFound("Event not found.");

        if (evt.Status != EventStatus.PendingApproval)
            return ServiceResult<EventDetailResponse>.Conflict(
                $"Event cannot be rejected because its current status is {evt.Status}.");

        evt.Status    = EventStatus.Rejected;
        evt.UpdatedAt = DateTime.UtcNow;
        await _eventRepo.UpdateAsync(evt);
        return ServiceResult<EventDetailResponse>.Ok(MapToDetail(evt));
    }

    public async Task<ServiceResult<EventDetailResponse>> CreateAsync(
        CreateEventRequest request, int organizerId)
    {
        if (request.EndTime <= request.StartTime)
            return ServiceResult<EventDetailResponse>.Failure("EndTime must be after StartTime.");

        if (request.Capacity <= 0)
            return ServiceResult<EventDetailResponse>.Failure("Capacity must be greater than 0.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
        if (category is null)
            return ServiceResult<EventDetailResponse>.Failure(
                $"Category with id {request.CategoryId} does not exist.");

        var venue = await _venueRepo.GetByIdAsync(request.VenueId);
        if (venue is null)
            return ServiceResult<EventDetailResponse>.Failure(
                $"Venue with id {request.VenueId} does not exist.");

        var evt = new Event
        {
            Title       = request.Title,
            Description = request.Description,
            StartTime   = request.StartTime,
            EndTime     = request.EndTime,
            Capacity    = request.Capacity,
            Status      = EventStatus.PendingApproval,
            OrganizerId = organizerId,
            CategoryId  = request.CategoryId,
            VenueId     = request.VenueId
        };

        await _eventRepo.CreateAsync(evt);

        // Reload with all navigation properties for response mapping
        var created = await _eventRepo.GetByIdWithDetailsAsync(evt.Id);
        if (created is null)
            return ServiceResult<EventDetailResponse>.Failure("Created event could not be loaded.");

        return ServiceResult<EventDetailResponse>.Ok(MapToDetail(created));
    }

    public async Task<ServiceResult<EventDetailResponse>> UpdateAsync(
        int id, UpdateEventRequest request, int callerId, string callerRole)
    {
        var evt = await _eventRepo.GetByIdWithDetailsAsync(id);
        if (evt is null)
            return ServiceResult<EventDetailResponse>.NotFound("Event not found.");

        bool isAdmin = callerRole == "Admin";
        if (!isAdmin && evt.OrganizerId != callerId)
            return ServiceResult<EventDetailResponse>.Forbidden(
                "You are not authorized to update this event.");

        if (request.EndTime <= request.StartTime)
            return ServiceResult<EventDetailResponse>.Failure("EndTime must be after StartTime.");

        if (request.Capacity <= 0)
            return ServiceResult<EventDetailResponse>.Failure("Capacity must be greater than 0.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
        if (category is null)
            return ServiceResult<EventDetailResponse>.Failure(
                $"Category with id {request.CategoryId} does not exist.");

        var venue = await _venueRepo.GetByIdAsync(request.VenueId);
        if (venue is null)
            return ServiceResult<EventDetailResponse>.Failure(
                $"Venue with id {request.VenueId} does not exist.");

        evt.Title       = request.Title;
        evt.Description = request.Description;
        evt.StartTime   = request.StartTime;
        evt.EndTime     = request.EndTime;
        evt.Capacity    = request.Capacity;
        evt.CategoryId  = request.CategoryId;
        evt.VenueId     = request.VenueId;
        evt.UpdatedAt   = DateTime.UtcNow;

        // Refresh navigation properties in case category/venue changed
        evt.Category = category;
        evt.Venue    = venue;

        await _eventRepo.UpdateAsync(evt);
        return ServiceResult<EventDetailResponse>.Ok(MapToDetail(evt));
    }

    public async Task<ServiceResult<bool>> CancelAsync(
        int id, int callerId, string callerRole)
    {
        var evt = await _eventRepo.GetByIdWithDetailsAsync(id);
        if (evt is null)
            return ServiceResult<bool>.NotFound("Event not found.");

        bool isAdmin = callerRole == "Admin";
        if (!isAdmin && evt.OrganizerId != callerId)
            return ServiceResult<bool>.Forbidden(
                "You are not authorized to cancel this event.");

        if (evt.Status == EventStatus.Cancelled)
            return ServiceResult<bool>.Conflict("Event is already cancelled.");

        evt.Status    = EventStatus.Cancelled;
        evt.UpdatedAt = DateTime.UtcNow;
        await _eventRepo.UpdateAsync(evt);
        return ServiceResult<bool>.Ok(true);
    }

    private static EventSummaryResponse MapToSummary(Event e) => new()
    {
        Id            = e.Id,
        Title         = e.Title,
        StartTime     = e.StartTime,
        EndTime       = e.EndTime,
        Capacity      = e.Capacity,
        Status        = e.Status.ToString(),
        OrganizerName = e.Organizer.FullName,
        CategoryName  = e.Category.Name,
        VenueName     = e.Venue.Name,
        VenueCity     = e.Venue.City
    };

    private static EventDetailResponse MapToDetail(Event e) => new()
    {
        Id                = e.Id,
        Title             = e.Title,
        Description       = e.Description,
        StartTime         = e.StartTime,
        EndTime           = e.EndTime,
        Capacity          = e.Capacity,
        Status            = e.Status.ToString(),
        CreatedAt         = e.CreatedAt,
        UpdatedAt         = e.UpdatedAt,
        OrganizerId       = e.OrganizerId,
        OrganizerName     = e.Organizer.FullName,
        CategoryId        = e.CategoryId,
        CategoryName      = e.Category.Name,
        VenueId           = e.VenueId,
        VenueName         = e.Venue.Name,
        VenueAddress      = e.Venue.Address,
        VenueCity         = e.Venue.City,
        RegistrationCount = e.Registrations.Count(r => r.Status == RegistrationStatus.Registered)
    };
}