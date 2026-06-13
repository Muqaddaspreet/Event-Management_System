namespace EventManagement.API.DTOs.Common;

public class ServiceResult<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public string Error { get; init; } = string.Empty;
    public bool IsConflict { get; init; }
    public bool IsNotFound { get; init; }

    public static ServiceResult<T> Ok(T data) =>
        new() { Succeeded = true, Data = data };

    public static ServiceResult<T> Conflict(string error) =>
        new() { Succeeded = false, Error = error, IsConflict = true };

    public static ServiceResult<T> NotFound(string error) =>
        new() { Succeeded = false, Error = error, IsNotFound = true };

    public static ServiceResult<T> Failure(string error) =>
        new() { Succeeded = false, Error = error };
}
