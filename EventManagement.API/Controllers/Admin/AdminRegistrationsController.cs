using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Registrations;
using EventManagement.API.Enums;
using EventManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.Admin;

[ApiController]
[Route("api/admin/registrations")]
[Authorize(Roles = "Admin")]
public class AdminRegistrationsController : ControllerBase
{
    private readonly IAdminService _adminService;
    public AdminRegistrationsController(IAdminService adminService) => _adminService = adminService;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminRegistrationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? eventId,
        [FromQuery] int? userId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        RegistrationStatus? parsedStatus = null;
        if (status is not null)
        {
            if (!Enum.TryParse<RegistrationStatus>(status, ignoreCase: true, out var s))
                return BadRequest(new ErrorResponse(
                    $"Invalid status value '{status}'. Valid values: Registered, Cancelled."));
            parsedStatus = s;
        }

        var result = await _adminService.GetRegistrationsPagedAsync(eventId, userId, parsedStatus, page, pageSize);
        return Ok(result);
    }
}