using EventManagement.API.Enums;

namespace EventManagement.API.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public EventStatus Status { get; set; } = EventStatus.PendingApproval;
    public int OrganizerId { get; set; }
    public int CategoryId { get; set; }
    public int VenueId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Organizer { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public ICollection<EventRegistration> Registrations { get; set; } = [];
}