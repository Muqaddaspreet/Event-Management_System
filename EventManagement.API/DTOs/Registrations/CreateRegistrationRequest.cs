using System.ComponentModel.DataAnnotations;

namespace EventManagement.API.DTOs.Registrations;

public class CreateRegistrationRequest
{
    [Required]
    public int EventId { get; set; }
}