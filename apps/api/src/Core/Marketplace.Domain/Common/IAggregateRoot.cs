namespace Marketplace.Domain.Common;

/// <summary>
/// Marker interface for aggregate roots — the only entities a Repository/DbSet
/// should be created for directly. Enforces the Aggregate boundary at compile time
/// (analyzers/reviews can flag repositories built against non-aggregate entities).
/// </summary>
public interface IAggregateRoot
{
}
