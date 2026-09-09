using FluentAssertions;
using FluentValidation.TestHelper;
using Marketplace.Application.Common.Diagnostics;
using Xunit;

namespace Marketplace.UnitTests.Application;

/// <summary>
/// The "one test for Validation" required by the Sprint 1.1 Definition of Done. Exercises
/// PingQueryValidator directly (fast, no HTTP/DI needed) — the equivalent end-to-end HTTP
/// case (a 400 ProblemDetails from a real request) is covered in the Integration test suite.
/// </summary>
public class PingQueryValidatorTests
{
    private readonly PingQueryValidator _validator = new();

    [Fact]
    public void Empty_message_fails_validation()
    {
        var result = _validator.TestValidate(new PingQuery(string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Message_longer_than_200_characters_fails_validation()
    {
        var tooLong = new string('a', 201);

        var result = _validator.TestValidate(new PingQuery(tooLong));

        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Valid_message_passes_validation()
    {
        var result = _validator.TestValidate(new PingQuery("hello"));

        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }
}
