using EventManagement.API.Enums;

namespace EventManagement.API.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Event> OrganizedEvents { get; set; } = [];
    public ICollection<EventRegistration> Registrations { get; set; } = [];
}