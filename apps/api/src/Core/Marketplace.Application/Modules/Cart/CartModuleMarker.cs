namespace Marketplace.Application.Modules.Cart;

/// <summary>
/// Assembly anchor for the Cart module's Application layer (Commands/Queries/Validators/DTOs
/// live under this namespace once the module's feature sprint starts). Also gives MediatR's
/// assembly scan a stable marker type independent of any single feature class.
/// </summary>
public static class CartModuleMarker
{
    public const string ModuleName = "Cart";
}
