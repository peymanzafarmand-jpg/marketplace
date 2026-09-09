namespace Marketplace.Application.Modules.Orders;

/// <summary>
/// Assembly anchor for the Orders module's Application layer (Commands/Queries/Validators/DTOs
/// live under this namespace once the module's feature sprint starts). Also gives MediatR's
/// assembly scan a stable marker type independent of any single feature class.
/// </summary>
public static class OrdersModuleMarker
{
    public const string ModuleName = "Orders";
}
