namespace Marketplace.Application.Modules.Payments;

/// <summary>
/// Assembly anchor for the Payments module's Application layer (Commands/Queries/Validators/DTOs
/// live under this namespace once the module's feature sprint starts). Also gives MediatR's
/// assembly scan a stable marker type independent of any single feature class.
/// </summary>
public static class PaymentsModuleMarker
{
    public const string ModuleName = "Payments";
}
