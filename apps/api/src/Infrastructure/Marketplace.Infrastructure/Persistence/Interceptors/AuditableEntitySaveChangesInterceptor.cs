using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Marketplace.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Stamps CreatedAt/CreatedBy/UpdatedAt/UpdatedBy on every AuditableEntity automatically on
/// SaveChanges — this is the ONLY place these fields are ever set (see Architecture doc
/// section "Database" audit fields requirement). Handlers/domain code must never set them.
/// </summary>
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUser, IDateTimeProvider dateTime)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = _dateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUser.UserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = _dateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUser.UserId;
                    break;
            }
        }
    }
}
