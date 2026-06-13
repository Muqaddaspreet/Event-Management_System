using System.ComponentModel.DataAnnotations;

namespace EventManagement.API.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [AllowedValues("Organizer", "Attendee", ErrorMessage = "Role must be 'Organizer' or 'Attendee'.")]
    public string Role { get; set; } = string.Empty;
}