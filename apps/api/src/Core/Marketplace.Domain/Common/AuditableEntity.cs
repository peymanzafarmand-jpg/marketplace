namespace Marketplace.Domain.Common;

/// <summary>
/// Adds standard audit fields. Populated automatically by
/// AuditableEntitySaveChangesInterceptor in the Infrastructure layer — never set manually.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
