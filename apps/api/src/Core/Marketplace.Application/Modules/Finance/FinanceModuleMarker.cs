namespace Marketplace.Application.Modules.Finance;

/// <summary>
/// Assembly anchor for the Finance module's Application layer (Commands/Queries/Validators/DTOs
/// live under this namespace once the module's feature sprint starts). Also gives MediatR's
/// assembly scan a stable marker type independent of any single feature class.
/// </summary>
public static class FinanceModuleMarker
{
    public const string ModuleName = "Finance";
}
