namespace EventManagement.API.DTOs.Venues;

public class VenueResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public int Capacity { get; init; }
}