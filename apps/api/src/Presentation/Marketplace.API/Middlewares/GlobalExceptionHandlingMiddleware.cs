using Marketplace.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ValidationException = Marketplace.Application.Common.Exceptions.ValidationException;

namespace Marketplace.API.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.GetCorrelationId();

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Access forbidden"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
        else
            _logger.LogWarning(exception, "Handled exception. CorrelationId: {CorrelationId}", correlationId);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = context.Request.Path,
            Extensions = { ["correlationId"] = correlationId }
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        if (statusCode != StatusCodes.Status500InternalServerError)
        {
            problemDetails.Detail = exception.Message;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        var payload = JsonSerializer.SerializeToUtf8Bytes(problemDetails);
        await context.Response.Body.WriteAsync(payload);
    }
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder) =>
        builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
}
