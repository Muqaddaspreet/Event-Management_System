namespace EventManagement.API.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}