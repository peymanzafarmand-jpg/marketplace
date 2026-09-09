using System.Security.Cryptography;
using System.Text;

namespace Marketplace.Infrastructure.Identity;

/// <summary>
/// SHA-256 hash used to store refresh tokens at rest (the raw token is only ever shown to
/// the client once, at issuance). Deterministic on purpose — refresh lookups are by hash
/// equality, so Argon2id-style salted hashing (used for passwords, see
/// Argon2idPasswordHasher) does not apply here.
/// </summary>
public static class TokenHasher
{
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
