using Asp.Versioning;

namespace Marketplace.API.Extensions;

/// <summary>URL-segment versioning ("/api/v1/..."), matching Architecture doc section 11.</summary>
public static class ApiVersioningExtensions
{
    public static IServiceCollection AddMarketplaceApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
