using Marketplace.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Marketplace.Infrastructure.Identity;

/// <summary>
/// Backs [Authorize(Policy = "permission_code")] policies (registered per-permission in
/// Marketplace.API/Extensions/AuthorizationExtensions). Checks the "permission" claims
/// embedded in the JWT by TokenService — no DB round-trip on the hot path.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }
    public PermissionRequirement(string permissionCode) => PermissionCode = permissionCode;
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasClaim = context.User.HasClaim("permission", requirement.PermissionCode);
        if (hasClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
