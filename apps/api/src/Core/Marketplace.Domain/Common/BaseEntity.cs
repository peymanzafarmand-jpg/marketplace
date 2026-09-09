namespace Marketplace.Domain.Common;

/// <summary>
/// Base class for every entity in the system. Uses a client-generated Guid so IDs are
/// stable across services/modules from day one (no dependency on a DB-assigned identity
/// column). Note: <c>Guid.NewGuid()</c> on .NET 8 produces a random RFC 4122 version-4
/// UUID, NOT a sequential/time-ordered one — it does not improve B-tree insert locality or
/// index fragmentation the way UUID v7 would. For high-volume, append-heavy tables added
/// later (per ADR-017 — e.g. <c>AuditLog</c>, <c>OrderStatusHistory</c>), generate UUID v7
/// explicitly at that table's creation time rather than assuming this base class provides
/// it; this class intentionally makes no ordering guarantee today.
/// </summary>
public abstract class BaseEntity : IEquatable<BaseEntity>
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>Domain events raised by this entity but not yet dispatched.</summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    protected void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool Equals(BaseEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as BaseEntity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity? left, BaseEntity? right) => Equals(left, right);

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !Equals(left, right);
}
