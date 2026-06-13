using EventManagement.API.Enums;

namespace EventManagement.API.Entities;

public class EventRegistration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Registered;
    public DateTime RegisteredAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}