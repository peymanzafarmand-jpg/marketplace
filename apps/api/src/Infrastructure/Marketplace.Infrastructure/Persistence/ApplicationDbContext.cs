using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Common;
using Marketplace.Domain.Modules.Identity;
using Marketplace.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Persistence;

/// <summary>
/// The single EF Core DbContext for the whole Modular Monolith (Shared Database, Shared
/// Schema — see Architecture doc 1.2). Each module's entities get their own
/// IEntityTypeConfiguration in Persistence/Configurations/{Module}; this class only wires
/// conventions (soft-delete filter, snake_case-friendly naming can be added per module)
/// and applies all configurations from this assembly.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // --- Identity module (only module with entities in this Foundation sprint) ---
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // --- Cross-cutting ---
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Every entity inherits BaseEntity, whose public `DomainEvents` collection
        // (IReadOnlyCollection<DomainEvent>) EF Core's convention-based discovery
        // otherwise tries to map as a navigation property to a new "DomainEvent"
        // entity type — which has no key configured, so model validation throws
        // ("The entity type 'DomainEvent' requires a primary key") the first time
        // any query actually runs. Confirmed by a real run during Task 1.7
        // verification (readiness check + the Outbox background service both hit
        // this on startup). DomainEvent is a transient, in-memory-only concept
        // (dispatched via MediatR in DispatchDomainEventsInterceptor, never
        // persisted) — it must never become part of the EF model at all.
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global soft-delete query filter: applied to every entity implementing ISoftDelete,
        // so a plain `_context.Set<T>()` query never has to remember to filter IsDeleted.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var condition = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(condition);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
