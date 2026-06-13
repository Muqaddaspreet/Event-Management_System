using EventManagement.API.DTOs.Common;
using EventManagement.API.DTOs.Venues;
using EventManagement.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var venues = await _venueService.GetAllAsync();
        return Ok(venues);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVenueRequest request)
    {
        var result = await _venueService.CreateAsync(request);

        if (!result.Succeeded)
            return BadRequest(new ErrorResponse(result.Error));

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVenueRequest request)
    {
        var result = await _venueService.UpdateAsync(id, request);

        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound) return NotFound(error);
            return BadRequest(error);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _venueService.DeleteAsync(id);

        if (!result.Succeeded)
        {
            var error = new ErrorResponse(result.Error);
            if (result.IsNotFound) return NotFound(error);
            if (result.IsConflict) return Conflict(error);
            return BadRequest(error);
        }

        return NoContent();
    }
}