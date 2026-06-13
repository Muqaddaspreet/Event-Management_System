using System.Security.Claims;
using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Registrations;
using EventManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers;

[ApiController]
[Route("api/registrations")]
[Authorize(Roles = "Attendee")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    // GET /api/registrations/mine — attendee's own registrations (all statuses)
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IEnumerable<RegistrationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine()
    {
        var result = await _registrationService.GetMineAsync(GetCallerId());
        return Ok(result);
    }

    // POST /api/registrations — register for a published event; reactivates cancelled record
    [HttpPost]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] CreateRegistrationRequest request)
    {
        var result = await _registrationService.RegisterAsync(request, GetCallerId());
        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound)            return NotFound(error);
            if (result.IsConflict)            return Conflict(error);
            if (result.IsUnprocessableEntity) return UnprocessableEntity(error);
            return BadRequest(error);
        }
        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    // DELETE /api/registrations/{id} — sets Status = Cancelled, no hard delete
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _registrationService.CancelAsync(id, GetCallerId());
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