namespace EventManagement.API.DTOs.Auth;

public class AuthResult
{
    public bool Succeeded { get; init; }
    public AuthResponse? Data { get; init; }
    public string Error { get; init; } = string.Empty;
    public bool IsConflict { get; init; }

    public static AuthResult Ok(AuthResponse data) =>
        new() { Succeeded = true, Data = data };

    public static AuthResult Conflict(string error) =>
        new() { Succeeded = false, Error = error, IsConflict = true };

    public static AuthResult Failure(string error) =>
        new() { Succeeded = false, Error = error };
}