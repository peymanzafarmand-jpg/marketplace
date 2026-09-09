using FluentAssertions;
using Marketplace.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Xunit;

namespace Marketplace.UnitTests.Infrastructure.Identity;

/// <summary>
/// Required by Task 1.5 Test Plan: Argon2id hasher must (a) produce a different hash for
/// the same password on repeated calls (proves a fresh random salt is used each time), (b)
/// verify correctly for the right password, and (c) reject the wrong password.
/// </summary>
public class Argon2idPasswordHasherTests
{
    private static Argon2idPasswordHasher CreateHasher(string pepper = "unit-test-pepper-not-for-production") =>
        new(Options.Create(new PasswordHashingOptions
        {
            Pepper = pepper,
            // Deliberately low cost parameters so the test suite stays fast — production
            // values (PasswordHashingOptions defaults) are set via configuration, not here.
            MemorySizeKb = 8192,
            Iterations = 1,
            DegreeOfParallelism = 2,
            HashLength = 16
        }));

    [Fact]
    public void Hashing_the_same_password_twice_produces_different_hashes()
    {
        var hasher = CreateHasher();

        var hash1 = hasher.Hash("Sup3rSecret!");
        var hash2 = hasher.Hash("Sup3rSecret!");

        hash1.Should().NotBe(hash2, "each hash must use a fresh random salt");
    }

    [Fact]
    public void Verify_succeeds_for_the_correct_password()
    {
        var hasher = CreateHasher();
        var hash = hasher.Hash("Sup3rSecret!");

        hasher.Verify("Sup3rSecret!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_fails_for_an_incorrect_password()
    {
        var hasher = CreateHasher();
        var hash = hasher.Hash("Sup3rSecret!");

        hasher.Verify("WrongPassword!", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_fails_when_the_pepper_differs()
    {
        var hash = CreateHasher(pepper: "pepper-one").Hash("Sup3rSecret!");

        CreateHasher(pepper: "pepper-two").Verify("Sup3rSecret!", hash).Should().BeFalse();
    }

    [Fact]
    public void Encoded_hash_is_self_describing_and_starts_with_argon2id()
    {
        var hasher = CreateHasher();

        var hash = hasher.Hash("Sup3rSecret!");

        hash.Should().StartWith("argon2id$v=19$m=8192,t=1,p=2$");
    }
}
