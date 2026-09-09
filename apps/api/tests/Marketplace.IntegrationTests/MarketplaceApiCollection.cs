using Xunit;

namespace Marketplace.IntegrationTests;

[CollectionDefinition(nameof(MarketplaceApiCollection))]
public class MarketplaceApiCollection : ICollectionFixture<MarketplaceApiFactory>
{
}
