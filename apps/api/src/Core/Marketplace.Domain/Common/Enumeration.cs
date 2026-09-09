namespace Marketplace.Domain.Common;

/// <summary>
/// Base class for rich enums (smallint-backed in the DB) that carry behavior/metadata beyond
/// a plain C# enum — e.g. OrderStatus, PaymentStatus. Use a plain `enum` when no behavior is
/// needed; use this when transitions/rules belong to the type itself.
/// </summary>
public abstract class Enumeration : IComparable
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(System.Reflection.BindingFlags.Public |
                             System.Reflection.BindingFlags.Static |
                             System.Reflection.BindingFlags.DeclaredOnly)
                 .Select(f => f.GetValue(null))
                 .Cast<T>();

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other) return false;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public int CompareTo(object? obj) => Id.CompareTo(((Enumeration)obj!).Id);
}
