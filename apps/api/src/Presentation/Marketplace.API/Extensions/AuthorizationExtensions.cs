using Marketplace.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Marketplace.API.Extensions;

/// <summary>
/// Bootstrap permission codes for this Foundation sprint. Each future module adds its own
/// codes here (or via a per-module extension) as its feature sprint lands — see Architecture
/// doc section 12.2. Policy name == permission code, so controllers just write
/// [Authorize(Policy = "order.view.all")].
/// </summary>
public static class AuthorizationExtensions
{
    private static readonly string[] BootstrapPermissionCodes =
    {
        "system.health.view",
        "admin.access"
    };

    public static IServiceCollection AddMarketplaceAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder();

        services.PostConfigure<AuthorizationOptions>(options =>
        {
            foreach (var code in BootstrapPermissionCodes)
            {
                options.AddPolicy(code, policy =>
                    policy.Requirements.Add(new PermissionRequirement(code)));
            }
        });

        return services;
    }
}
