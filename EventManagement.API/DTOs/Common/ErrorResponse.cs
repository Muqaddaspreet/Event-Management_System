namespace EventManagement.API.DTOs.Common;

public class ErrorResponse
{
    public string Message { get; init; } = string.Empty;
    public string[]? Errors { get; init; }

    public ErrorResponse(string message, string[]? errors = null)
    {
        Message = message;
        Errors = errors;
    }
}