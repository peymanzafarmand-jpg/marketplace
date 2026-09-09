using Serilog;

namespace Marketplace.API.Middlewares;

/// <summary>
/// Wraps Serilog.AspNetCore's RequestLoggingMiddleware with a message template that never
/// includes query string or body (both can carry sensitive data) — only method, path
/// (template, not raw — avoids high-cardinality IDs in the log message itself), status and
/// elapsed time. Full structured properties are still attached for querying in Seq/ELK.
/// </summary>
public static class RequestLoggingExtensions
{
    public static IApplicationBuilder UseMarketplaceRequestLogging(this IApplicationBuilder builder) =>
        builder.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("CorrelationId", httpContext.GetCorrelationId());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            };
        });
}
