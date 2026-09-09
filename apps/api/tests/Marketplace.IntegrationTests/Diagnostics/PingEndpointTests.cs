using System.Net;
using FluentAssertions;
using Xunit;

namespace Marketplace.IntegrationTests.Diagnostics;

[Collection(nameof(MarketplaceApiCollection))]
public class PingEndpointTests
{
    private readonly MarketplaceApiFactory _factory;

    public PingEndpointTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Ping_with_a_valid_message_returns_200_with_the_echoed_value()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/diagnostics/ping?message=hello");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("pong: hello");
    }

    /// <summary>
    /// The required "test for Global Exception Handling" — an empty message fails
    /// PingQueryValidator, MediatR's ValidationBehavior throws ValidationException, and
    /// GlobalExceptionHandlingMiddleware must turn that into a 400 ProblemDetails
    /// (not an unhandled 500, and not a raw stack trace).
    /// </summary>
    [Fact]
    public async Task Ping_with_an_empty_message_returns_400_problem_details()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/diagnostics/ping?message=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("correlationId");
        body.Should().Contain("errors");
    }
}
