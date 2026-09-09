namespace Marketplace.Infrastructure.Identity;

/// <summary>
/// Bound from configuration ("Jwt" section / environment variables). Per ADR-014, tokens
/// are signed with RS256 (asymmetric) rather than HS256 — the private key only needs to
/// live on the service(s) that ISSUE tokens; any future service that only needs to VERIFY
/// tokens can be handed the public key (or the JWKS endpoint, once published — see
/// TokenService's TODO) without ever holding signing capability. Key file paths match
/// JWT_PRIVATE_KEY_PATH / JWT_PUBLIC_KEY_PATH already defined in the Security team's
/// .env.example. There is deliberately no symmetric "Secret" field anymore.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Filesystem path to the RSA private key, PEM format (PKCS#1 or PKCS#8).</summary>
    public string PrivateKeyPath { get; set; } = default!;

    /// <summary>Filesystem path to the matching RSA public key, PEM format.</summary>
    public string PublicKeyPath { get; set; } = default!;

    public string Issuer { get; set; } = "Marketplace.API";
    public string Audience { get; set; } = "Marketplace.Clients";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}
