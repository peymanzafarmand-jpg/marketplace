using Marketplace.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Common.Interfaces;

/// <summary>
/// Application-layer view of the DbContext (Dependency Inversion: Application depends on
/// this interface, Infrastructure's ApplicationDbContext implements it). Only aggregate
/// roots that already exist are exposed here; each module adds its own DbSets to this
/// interface when its feature sprint starts.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
