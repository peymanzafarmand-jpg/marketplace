using Serilog.Context;

namespace Marketplace.API.Middlewares;

/// <summary>
/// Reads/generates an X-Correlation-Id per request, pushes it into the Serilog LogContext
/// (so every log line for this request carries it) and echoes it back on the response
/// header — lets the client and backend correlate a support ticket to exact log lines.
/// Must run before request logging and the exception handler in the pipeline.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder) =>
        builder.UseMiddleware<CorrelationIdMiddleware>();

    public static string GetCorrelationId(this HttpContext context) =>
        context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
}
