using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using FluentAssertions;
using Marketplace.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Marketplace.UnitTests.Infrastructure.Identity;

/// <summary>
/// Required by Task 1.5 Test Plan: the token produced by TokenService must (a) verify
/// successfully against the matching RSA PUBLIC key (proves RS256/asymmetric signing is
/// actually happening, not just configured), and (b) must NOT contain a phone_number claim
/// — the explicit negative test for Security doc section 2.2.
/// </summary>
public class TokenServiceTests : IDisposable
{
    private readonly string _tempDir = Directory.CreateTempSubdirectory("token-service-tests-").FullName;
    private readonly string _privateKeyPath;
    private readonly string _publicKeyPath;
    private readonly RSA _rsa = RSA.Create(2048);

    public TokenServiceTests()
    {
        _privateKeyPath = Path.Combine(_tempDir, "private.pem");
        _publicKeyPath = Path.Combine(_tempDir, "public.pem");

        File.WriteAllText(_privateKeyPath, _rsa.ExportRSAPrivateKeyPem());
        File.WriteAllText(_publicKeyPath, _rsa.ExportRSAPublicKeyPem());
    }

    private TokenService CreateTokenService() => new(Options.Create(new JwtOptions
    {
        PrivateKeyPath = _privateKeyPath,
        PublicKeyPath = _publicKeyPath,
        Issuer = "Marketplace.API.Tests",
        Audience = "Marketplace.Clients.Tests",
        AccessTokenMinutes = 15
    }));

    [Fact]
    public void Generated_token_is_signed_with_RS256_and_verifies_against_the_public_key()
    {
        using var tokenService = CreateTokenService();
        var token = tokenService.GenerateAccessToken(Guid.NewGuid(), new[] { "order.view.all" });

        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Marketplace.API.Tests",
            ValidateAudience = true,
            ValidAudience = "Marketplace.Clients.Tests",
            ValidateLifetime = true,
            IssuerSigningKey = new RsaSecurityKey(_rsa),
            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }
        };

        var act = () => handler.ValidateToken(token, validationParameters, out _);

        act.Should().NotThrow("the token must verify against the matching RSA public key using RS256");
    }

    [Fact]
    public void Generated_token_does_not_contain_a_phone_number_claim()
    {
        using var tokenService = CreateTokenService();
        var token = tokenService.GenerateAccessToken(Guid.NewGuid(), new[] { "order.view.all" });

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().NotContain(c => c.Type == "phone_number");
        jwt.Claims.Should().NotContain(c => c.Type == ClaimTypes.MobilePhone);
        jwt.Claims.Should().NotContain(c => c.Type == JwtRegisteredClaimNames.Email);
    }

    [Fact]
    public void Generated_token_contains_only_the_allowed_claim_types()
    {
        using var tokenService = CreateTokenService();
        var token = tokenService.GenerateAccessToken(Guid.NewGuid(), new[] { "order.view.all", "product.create" });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var allowedTypes = new[]
        {
            JwtRegisteredClaimNames.Sub, JwtRegisteredClaimNames.Jti, "permission",
            // Standard JWT registered claims added automatically by the token handler:
            JwtRegisteredClaimNames.Iss, JwtRegisteredClaimNames.Aud,
            JwtRegisteredClaimNames.Exp, JwtRegisteredClaimNames.Nbf, JwtRegisteredClaimNames.Iat
        };

        jwt.Claims.Select(c => c.Type).Should().OnlyContain(type => allowedTypes.Contains(type));
    }

    public void Dispose()
    {
        _rsa.Dispose();
        Directory.Delete(_tempDir, recursive: true);
    }
}
