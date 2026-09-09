namespace Marketplace.Domain.Common;

/// <summary>
/// Implement on any entity that should be soft-deleted instead of physically removed.
/// A global EF Core query filter (configured in ApplicationDbContext) automatically
/// excludes rows where IsDeleted = true from every query.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}
