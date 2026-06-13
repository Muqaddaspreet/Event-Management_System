using System.ComponentModel.DataAnnotations;

namespace EventManagement.API.DTOs.Events;

public class CreateEventRequest
{
    [Required][MaxLength(200)] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public DateTime StartTime { get; set; }
    [Required] public DateTime EndTime { get; set; }
    [Range(1, int.MaxValue)] public int Capacity { get; set; }
    [Required] public int CategoryId { get; set; }
    [Required] public int VenueId { get; set; }
}