using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Marketplace.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Marketplace.Infrastructure.Identity;

/// <summary>
/// Argon2id password hasher (replaces the earlier BCrypt implementation per Sprint 1
/// Architecture Review — Argon2id is the OWASP-recommended default for new systems:
/// memory-hard, resistant to GPU/ASIC cracking in a way BCrypt is not).
///
/// Storage format (single string, self-describing so parameters can change later without
/// breaking verification of already-stored hashes):
///   argon2id$v=19$m={memoryKb},t={iterations},p={parallelism}${saltBase64}${hashBase64}
///
/// The pepper (PasswordHashingOptions.Pepper, a server-side secret — see that class's
/// remarks) is appended to the password before hashing and is NEVER stored in the encoded
/// string or the database; without it, even a full database + this code cannot recompute
/// a valid hash for a known password.
/// </summary>
public class Argon2idPasswordHasher : IPasswordHasher
{
    private const string Prefix = "argon2id";
    private const int SaltSize = 16;

    private readonly PasswordHashingOptions _options;

    public Argon2idPasswordHasher(IOptions<PasswordHashingOptions> options)
    {
        _options = options.Value;
    }

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(password, salt, _options.MemorySizeKb, _options.Iterations, _options.DegreeOfParallelism, _options.HashLength);

        return Encode(_options.MemorySizeKb, _options.Iterations, _options.DegreeOfParallelism, salt, hash);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (!TryDecode(passwordHash, out var memoryKb, out var iterations, out var parallelism, out var salt, out var expectedHash))
            return false;

        var actualHash = ComputeHash(password, salt, memoryKb, iterations, parallelism, expectedHash.Length);

        // Constant-time comparison — avoids leaking hash-match progress via timing.
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private byte[] ComputeHash(
        string password, byte[] salt, int memoryKb, int iterations, int parallelism, int hashLength)
    {
        var pepperedPassword = Encoding.UTF8.GetBytes(password + _options.Pepper);

        using var argon2 = new Argon2id(pepperedPassword)
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            MemorySize = memoryKb,
            Iterations = iterations
        };

        return argon2.GetBytes(hashLength);
    }

    private static string Encode(int memoryKb, int iterations, int parallelism, byte[] salt, byte[] hash) =>
        $"{Prefix}$v=19$m={memoryKb},t={iterations},p={parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";

    private static bool TryDecode(
        string encoded, out int memoryKb, out int iterations, out int parallelism, out byte[] salt, out byte[] hash)
    {
        memoryKb = 0; iterations = 0; parallelism = 0; salt = Array.Empty<byte>(); hash = Array.Empty<byte>();

        var parts = encoded.Split('$', StringSplitOptions.RemoveEmptyEntries);
        // parts: [ "argon2id", "v=19", "m=...,t=...,p=...", "<salt>", "<hash>" ]
        if (parts.Length != 5 || parts[0] != Prefix) return false;

        var paramParts = parts[2].Split(',');
        if (paramParts.Length != 3) return false;

        try
        {
            memoryKb = int.Parse(paramParts[0].Split('=')[1]);
            iterations = int.Parse(paramParts[1].Split('=')[1]);
            parallelism = int.Parse(paramParts[2].Split('=')[1]);
            salt = Convert.FromBase64String(parts[3]);
            hash = Convert.FromBase64String(parts[4]);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
