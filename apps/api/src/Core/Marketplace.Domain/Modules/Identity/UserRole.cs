using Marketplace.Domain.Common;

namespace Marketplace.Domain.Modules.Identity;

/// <summary>Join entity, User <-> Role (composite key configured in Infrastructure).</summary>
public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    private UserRole() { } // EF Core

    public static UserRole Create(Guid userId, Guid roleId) =>
        new() { UserId = userId, RoleId = roleId };
}
