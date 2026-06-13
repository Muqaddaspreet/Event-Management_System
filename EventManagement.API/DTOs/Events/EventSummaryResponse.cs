namespace EventManagement.API.DTOs.Events;

public class EventSummaryResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public int Capacity { get; init; }
    public string Status { get; init; } = string.Empty;
    public string OrganizerName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string VenueName { get; init; } = string.Empty;
    public string VenueCity { get; init; } = string.Empty;
}