using System.Net;
using FluentAssertions;
using Xunit;

namespace Marketplace.IntegrationTests.Diagnostics;

/// <summary>The required "successful test for Health Check" from the Sprint 1.1 Definition of Done.</summary>
[Collection(nameof(MarketplaceApiCollection))]
public class HealthCheckTests
{
    private readonly MarketplaceApiFactory _factory;

    public HealthCheckTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/health/live")]
    public async Task Health_endpoint_returns_200_ok(string path)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
