using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Events;
using EventManagement.API.Enums;
using EventManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.Admin;

[ApiController]
[Route("api/admin/events")]
[Authorize(Roles = "Admin")]
public class AdminEventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public AdminEventsController(IEventService eventService) => _eventService = eventService;

    // GET /api/admin/events — all events, all statuses, optional filters
    // Invalid status string returns 400; valid status string filters by that status.
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EventSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? organizerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        EventStatus? parsedStatus = null;
        if (status is not null)
        {
            if (!Enum.TryParse<EventStatus>(status, ignoreCase: true, out var s))
                return BadRequest(new ErrorResponse($"Invalid status value '{status}'. Valid values: PendingApproval, Published, Rejected, Cancelled."));
            parsedStatus = s;
        }

        var result = await _eventService.GetAllForAdminAsync(parsedStatus, organizerId, page, pageSize);
        return Ok(result);
    }

    // POST /api/admin/events/{id}/approve — PendingApproval → Published
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _eventService.ApproveAsync(id);
        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound) return NotFound(error);
            if (result.IsConflict) return Conflict(error);
            return BadRequest(error);
        }
        return Ok(result.Data);
    }

    // POST /api/admin/events/{id}/reject — PendingApproval → Rejected
    // Reason is accepted in the request body but not persisted in V1 (no DB column).
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectEventRequest request)
    {
        var result = await _eventService.RejectAsync(id, request);
        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound) return NotFound(error);
            if (result.IsConflict) return Conflict(error);
            return BadRequest(error);
        }
        return Ok(result.Data);
    }
}