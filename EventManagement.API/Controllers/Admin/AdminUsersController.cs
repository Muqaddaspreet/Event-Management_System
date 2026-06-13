using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Users;
using EventManagement.API.Enums;
using EventManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminService _adminService;
    public AdminUsersController(IAdminService adminService) => _adminService = adminService;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        UserRole? parsedRole = null;
        if (role is not null)
        {
            if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var r))
                return BadRequest(new ErrorResponse(
                    $"Invalid role value '{role}'. Valid values: Admin, Organizer, Attendee."));
            parsedRole = r;
        }

        var result = await _adminService.GetUsersPagedAsync(parsedRole, search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _adminService.GetUserByIdAsync(id);
        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound) return NotFound(error);
            return BadRequest(error);
        }
        return Ok(result.Data);
    }
}