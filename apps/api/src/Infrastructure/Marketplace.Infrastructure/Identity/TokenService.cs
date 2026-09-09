using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Marketplace.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Marketplace.Infrastructure.Identity;

/// <summary>
/// Issues short-lived JWT access tokens signed with RS256 (ADR-014 — asymmetric, not
/// HS256/symmetric) and cryptographically random refresh tokens. Refresh token
/// hashing/rotation/persistence remains the caller's responsibility (RefreshToken domain
/// entity + repository) — this service only generates raw values.
///
/// Claims deliberately exclude any direct user-identifying data (phone number, email) —
/// per Security doc section 2.2, a JWT is signed but NOT encrypted, so anyone holding the
/// token (including client-side JS, browser devtools, or a logging pipeline that
/// accidentally captures an Authorization header) can trivially base64-decode the payload.
/// The only allowed claims are: sub (user id), jti (token id), permission (0..N), and
/// optionally role. Look up phone/email server-side from UserId when actually needed.
///
/// TODO(Sprint 2+): publish the public key via a JWKS endpoint (/.well-known/jwks.json) so
/// other services can verify tokens without being handed the raw PEM file. Not implemented
/// this sprint because no token-consuming service exists yet outside this API itself.
/// </summary>
public class TokenService : ITokenService, IDisposable
{
    private readonly JwtOptions _options;
    private readonly Lazy<RSA> _signingKey;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _signingKey = new Lazy<RSA>(LoadPrivateKey);
    }

    private RSA LoadPrivateKey()
    {
        if (string.IsNullOrWhiteSpace(_options.PrivateKeyPath) || !File.Exists(_options.PrivateKeyPath))
        {
            throw new InvalidOperationException(
                $"JWT private key not found at '{_options.PrivateKeyPath}'. Set Jwt:PrivateKeyPath " +
                "(JWT_PRIVATE_KEY_PATH) to a PEM-encoded RSA private key file.");
        }

        var pem = File.ReadAllText(_options.PrivateKeyPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return rsa;
    }

    public string GenerateAccessToken(Guid userId, IEnumerable<string> permissionCodes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(permissionCodes.Select(code => new Claim("permission", code)));

        var credentials = new SigningCredentials(new RsaSecurityKey(_signingKey.Value), SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public void Dispose()
    {
        if (_signingKey.IsValueCreated)
            _signingKey.Value.Dispose();
    }
}
