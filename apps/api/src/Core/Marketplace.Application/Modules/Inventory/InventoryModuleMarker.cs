namespace Marketplace.Application.Modules.Inventory;

/// <summary>
/// Assembly anchor for the Inventory module's Application layer (Commands/Queries/Validators/DTOs
/// live under this namespace once the module's feature sprint starts). Also gives MediatR's
/// assembly scan a stable marker type independent of any single feature class.
/// </summary>
public static class InventoryModuleMarker
{
    public const string ModuleName = "Inventory";
}
