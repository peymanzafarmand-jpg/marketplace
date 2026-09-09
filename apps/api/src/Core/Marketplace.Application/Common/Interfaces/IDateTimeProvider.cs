namespace Marketplace.Application.Common.Interfaces;

/// <summary>Testable wall clock — never call DateTimeOffset.UtcNow directly from Application code.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
