using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Marketplace.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Converts a hard Remove() on any ISoftDelete entity into an update
/// (IsDeleted/DeletedAt/DeletedBy) instead — see Architecture doc "Soft Delete Strategy".
/// Combined with the global query filter in ApplicationDbContext, callers never need to
/// think about soft delete explicitly.
/// </summary>
public class SoftDeleteSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public SoftDeleteSaveChangesInterceptor(ICurrentUserService currentUser, IDateTimeProvider dateTime)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplySoftDelete(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State != EntityState.Deleted) continue;

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = _dateTime.UtcNow;
            entry.Entity.DeletedBy = _currentUser.UserId;
        }
    }
}
