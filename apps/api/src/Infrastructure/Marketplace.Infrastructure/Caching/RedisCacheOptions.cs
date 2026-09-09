namespace Marketplace.Infrastructure.Caching;

public class RedisCacheOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "marketplace:";
}
