using EventManagement.API.DTOs.Admin;
using EventManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    public AdminDashboardController(IAdminDashboardService dashboardService)
        => _dashboardService = dashboardService;

    [HttpGet]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await _dashboardService.GetDashboardAsync();
        return Ok(result);
    }
}