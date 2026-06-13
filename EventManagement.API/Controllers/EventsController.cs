using System.Security.Claims;
using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Events;
using EventManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // GET /api/events — public, Published only, with optional filters
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublished(
        [FromQuery] int? categoryId,
        [FromQuery] int? venueId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _eventService.GetPublishedAsync(categoryId, venueId, search, page, pageSize);
        return Ok(result);
    }

    // GET /api/events/mine — Organizer only, own events across all statuses
    [HttpGet("mine")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(PagedResult<EventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _eventService.GetByOrganizerAsync(GetCallerId(), page, pageSize);
        return Ok(result);
    }

    // GET /api/events/{id} — visibility enforced in service (404 for inaccessible events)
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        int? callerId  = User.Identity?.IsAuthenticated == true ? GetCallerId() : null;
        string? role   = User.FindFirstValue(ClaimTypes.Role);

        var result = await _eventService.GetByIdAsync(id, callerId, role);
        if (!result.Succeeded)
            return NotFound(new ErrorResponse(result.Error));

        return Ok(result.Data);
    }

    // POST /api/events — Organizer only, defaults to PendingApproval
    [HttpPost]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var result = await _eventService.CreateAsync(request, GetCallerId());
        if (!result.Succeeded)
            return BadRequest(new ErrorResponse(result.Error));

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    // PUT /api/events/{id} — Organizer (own) or Admin
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventRequest request)
    {
        var result = await _eventService.UpdateAsync(
            id, request, GetCallerId(), User.FindFirstValue(ClaimTypes.Role)!);

        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound)  return NotFound(error);
            if (result.IsForbidden) return StatusCode(StatusCodes.Status403Forbidden, error);
            return BadRequest(error);
        }

        return Ok(result.Data);
    }

    // DELETE /api/events/{id} — sets Status = Cancelled, no hard delete
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organizer,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _eventService.CancelAsync(
            id, GetCallerId(), User.FindFirstValue(ClaimTypes.Role)!);

        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound)  return NotFound(error);
            if (result.IsForbidden) return StatusCode(StatusCodes.Status403Forbidden, error);
            if (result.IsConflict)  return Conflict(error);
            return BadRequest(error);
        }

        return NoContent();
    }

    private int GetCallerId() => int.Parse(User.FindFirstValue("userId")!);
}