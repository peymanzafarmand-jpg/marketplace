using Marketplace.Domain.Common;

namespace Marketplace.Domain.Modules.Identity;

public class Role : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public bool IsSystem { get; private set; }

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { } // EF Core

    public static Role Create(string name, bool isSystem = false) =>
        new() { Name = name, IsSystem = isSystem };

    public void GrantPermission(Permission permission)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permission.Id)) return;
        _rolePermissions.Add(RolePermission.Create(Id, permission.Id));
    }
}
