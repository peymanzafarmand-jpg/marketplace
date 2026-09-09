using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Xunit;

namespace Marketplace.IntegrationTests.Orders;

/// <summary>
/// Placeholder — the Orders module itself does not exist yet (Sprint 2+), so these
/// cases cannot run today. Kept here (rather than as a design doc) so the required
/// coverage lands in the same project structure real tests will use, wired to the
/// project's actual shared fixture (<see cref="MarketplaceApiFactory"/> via
/// <see cref="MarketplaceApiCollection"/> — real Testcontainers Postgres/Redis/RabbitMQ,
/// same pattern as <c>Diagnostics/HealthCheckTests.cs</c>), not a bare, unconfigured
/// WebApplicationFactory that would fail to start.
///
/// Documents the *expected* security regression coverage (IDOR) that must exist before
/// the Orders module ships — see docs/security/threat-model.md, section IDOR.
/// </summary>
[Collection(nameof(MarketplaceApiCollection))]
public class OrderAccessControlTests
{
    private readonly MarketplaceApiFactory _factory;

    public OrderAccessControlTests(MarketplaceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact(Skip = "Enable once Orders module + auth test helpers are implemented")]
    public async Task GetOrder_ShouldReturn403_WhenAccessingAnotherUsersOrder()
    {
        var client = _factory.CreateClient();

        // Arrange: token for "customer A", order belongs to "customer B"
        var tokenForCustomerA = "TODO: obtain via test auth helper";
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenForCustomerA);

        var otherUsersOrderId = Guid.NewGuid(); // TODO: seed as belonging to customer B

        // Act
        var response = await client.GetAsync($"/api/v1/orders/{otherUsersOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(Skip = "Enable once Orders module + auth test helpers are implemented")]
    public async Task GetOrder_ShouldReturn200_WhenAccessingOwnOrder()
    {
        var client = _factory.CreateClient();
        var tokenForCustomerA = "TODO: obtain via test auth helper";
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenForCustomerA);

        var ownOrderId = Guid.NewGuid(); // TODO: seed as belonging to customer A

        var response = await client.GetAsync($"/api/v1/orders/{ownOrderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
