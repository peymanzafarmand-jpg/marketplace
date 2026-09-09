using Marketplace.API.Common;
using Marketplace.API.Middlewares;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ApiResponse<T> Envelope<T>(T data, string? message = null) =>
        ApiResponse.Ok(data, HttpContext.GetCorrelationId(), message);
}
