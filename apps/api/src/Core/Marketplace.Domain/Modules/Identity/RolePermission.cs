using Marketplace.Domain.Common;

namespace Marketplace.Domain.Modules.Identity;

/// <summary>Join entity, Role <-> Permission (composite key configured in Infrastructure).</summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    private RolePermission() { } // EF Core

    public static RolePermission Create(Guid roleId, Guid permissionId) =>
        new() { RoleId = roleId, PermissionId = permissionId };
}
