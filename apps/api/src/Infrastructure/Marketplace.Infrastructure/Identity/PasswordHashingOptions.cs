namespace Marketplace.Infrastructure.Identity;

/// <summary>
/// Bound from configuration ("PasswordHashing" section / environment variables). Pepper is
/// a server-side secret ADDED TO the password before hashing — unlike the per-hash salt
/// (which Argon2id generates and stores alongside the hash automatically), the pepper is
/// never stored in the database at all, so a stolen DB dump alone cannot be brute-forced
/// even with the salts. Must come from the Secret Manager/.env in every environment beyond
/// local dev — matches PASSWORD_PEPPER already defined in the Security team's .env.example.
/// </summary>
public class PasswordHashingOptions
{
    public const string SectionName = "PasswordHashing";

    public string Pepper { get; set; } = default!;

    /// <summary>Degree of parallelism (lanes). Argon2id default guidance: 2x-4x core count, capped reasonably.</summary>
    public int DegreeOfParallelism { get; set; } = 4;

    /// <summary>Memory cost in KiB. 19 MiB (19456) is the current OWASP-recommended floor for Argon2id.</summary>
    public int MemorySizeKb { get; set; } = 19456;

    /// <summary>Number of passes over memory.</summary>
    public int Iterations { get; set; } = 2;

    /// <summary>Output hash length in bytes.</summary>
    public int HashLength { get; set; } = 32;
}
