namespace Marketplace.API.Common;

/// <summary>
/// Standard success envelope for every 2xx API response, so every client (web, mobile,
/// future partners) parses responses the same way. Errors do NOT use this envelope — they
/// use RFC 7807 ProblemDetails instead (see Middlewares/GlobalExceptionHandlingMiddleware),
/// which is the .NET/HTTP-native error format and keeps success/error shapes distinct.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string CorrelationId { get; init; } = default!;

    public static ApiResponse<T> Ok(T data, string correlationId, string? message = null) =>
        new() { Data = data, Message = message, CorrelationId = correlationId };
}

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string correlationId, string? message = null) =>
        ApiResponse<T>.Ok(data, correlationId, message);
}
