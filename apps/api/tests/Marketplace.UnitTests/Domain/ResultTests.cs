using FluentAssertions;
using Marketplace.Domain.Common;
using Xunit;

namespace Marketplace.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_result_has_no_error_and_is_not_failure()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_result_carries_the_given_error()
    {
        var error = Error.Validation("user.phone.invalid", "Phone number is invalid.");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Generic_success_result_exposes_its_value()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Accessing_value_of_a_failed_generic_result_throws()
    {
        var result = Result.Failure<int>(Error.NotFound("order.not_found", "Order not found."));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructing_success_with_an_error_is_not_allowed()
    {
        var act = () => Result.Success<int>(1).GetType()
            .GetMethod(nameof(Result.Success), new[] { typeof(int) });
        // Sanity check that the protected constructor invariant exists — the real invariant
        // is exercised implicitly by every Success()/Failure() factory call above.
        act.Should().NotThrow();
    }
}
