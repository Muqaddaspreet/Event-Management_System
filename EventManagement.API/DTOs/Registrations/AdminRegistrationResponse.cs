namespace EventManagement.API.DTOs.Registrations;

public class AdminRegistrationResponse
{
    public int Id { get; init; }
    public int EventId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public int UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}