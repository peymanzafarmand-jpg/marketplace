using System.Security.Claims;
using Marketplace.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Marketplace.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public IReadOnlyCollection<string> Permissions =>
        Principal?.FindAll("permission").Select(c => c.Value).ToList() ?? new List<string>();

    public bool HasPermission(string permissionCode) => Permissions.Contains(permissionCode);
}
