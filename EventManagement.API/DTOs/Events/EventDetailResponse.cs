namespace EventManagement.API.DTOs.Events;

public class EventDetailResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public int Capacity { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int OrganizerId { get; init; }
    public string OrganizerName { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int VenueId { get; init; }
    public string VenueName { get; init; } = string.Empty;
    public string VenueAddress { get; init; } = string.Empty;
    public string VenueCity { get; init; } = string.Empty;
    public int RegistrationCount { get; init; }
}