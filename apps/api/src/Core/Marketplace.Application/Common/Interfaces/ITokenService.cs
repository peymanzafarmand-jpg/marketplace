namespace Marketplace.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Issues a short-lived, RS256-signed JWT access token (ADR-014) carrying only
    /// non-identifying claims: sub (userId), jti, and permission (0..N). Per Security doc
    /// section 2.2, phone number/email/any other direct identifier must NEVER be embedded
    /// here — a JWT is signed, not encrypted, so its payload is trivially readable by
    /// anyone holding the token. Callers needing the user's phone/email should look it up
    /// server-side by UserId instead of reading it off the token.
    /// </summary>
    string GenerateAccessToken(Guid userId, IEnumerable<string> permissionCodes);

    /// <summary>Issues a cryptographically random refresh token (raw value — caller hashes it before storing).</summary>
    string GenerateRefreshToken();
}
