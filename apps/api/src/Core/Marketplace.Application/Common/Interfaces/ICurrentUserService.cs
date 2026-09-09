namespace Marketplace.Application.Common.Interfaces;

/// <summary>
/// Reads the authenticated user's identity out of the current HTTP context/JWT claims.
/// Deliberately exposes no PhoneNumber/Email property — per ADR/Security doc section 2.2,
/// those never live in the token's claims in the first place (see ITokenService). Code that
/// needs a user's contact details must fetch them server-side via UserId (e.g. from the
/// Users module) rather than expect them here.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool HasPermission(string permissionCode);
}
