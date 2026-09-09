using Marketplace.Domain.Common;

namespace Marketplace.Domain.Modules.Identity;

/// <summary>
/// Fine-grained permission, e.g. "product.create", "order.view.all", "settlement.approve".
/// Codes are seeded per module as each module's feature sprint lands; this Foundation
/// sprint only creates the table/shape and a couple of bootstrap codes.
/// </summary>
public class Permission : AuditableEntity, IAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string? Description { get; private set; }

    private Permission() { } // EF Core

    public static Permission Create(string code, string? description = null) =>
        new() { Code = code, Description = description };
}
