using Asp.Versioning;
using Marketplace.Application.Common.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiVersion("1.0")]
public class DiagnosticsController : ApiControllerBase
{
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ping([FromQuery] string? message, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new PingQuery(message ?? string.Empty), cancellationToken);
        return Ok(Envelope(result));
    }
}
