using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Events;
using EventManagement.API.Enums;

namespace EventManagement.API.Services.Interfaces;

public interface IEventService
{
    Task<PagedResult<EventSummaryResponse>> GetPublishedAsync(
        int? categoryId, int? venueId, string? search, int page, int pageSize);

    Task<ServiceResult<EventDetailResponse>> GetByIdAsync(
        int id, int? callerId, string? callerRole);

    Task<PagedResult<EventSummaryResponse>> GetByOrganizerAsync(
        int organizerId, int page, int pageSize);

    Task<PagedResult<EventSummaryResponse>> GetAllForAdminAsync(
        EventStatus? status, int? organizerId, int page, int pageSize);

    Task<ServiceResult<EventDetailResponse>> CreateAsync(
        CreateEventRequest request, int organizerId);

    Task<ServiceResult<EventDetailResponse>> UpdateAsync(
        int id, UpdateEventRequest request, int callerId, string callerRole);

    Task<ServiceResult<bool>> CancelAsync(
        int id, int callerId, string callerRole);

    Task<ServiceResult<EventDetailResponse>> ApproveAsync(int id);

    Task<ServiceResult<EventDetailResponse>> RejectAsync(int id, RejectEventRequest request);
}