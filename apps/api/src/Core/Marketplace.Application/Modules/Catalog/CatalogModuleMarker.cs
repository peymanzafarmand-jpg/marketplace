namespace Marketplace.Application.Modules.Catalog;

/// <summary>
/// Assembly anchor for the Catalog module's Application layer (Commands/Queries/Validators/DTOs
/// live under this namespace once the module's feature sprint starts). Also gives MediatR's
/// assembly scan a stable marker type independent of any single feature class.
/// </summary>
public static class CatalogModuleMarker
{
    public const string ModuleName = "Catalog";
}
