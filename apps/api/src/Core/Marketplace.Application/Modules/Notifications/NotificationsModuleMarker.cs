namespace Marketplace.Application.Modules.Notifications;

/// <summary>
/// Assembly anchor for the Notifications module's Application layer (Commands/Queries/Validators/DTOs
/// live under this namespace once the module's feature sprint starts). Also gives MediatR's
/// assembly scan a stable marker type independent of any single feature class.
/// </summary>
public static class NotificationsModuleMarker
{
    public const string ModuleName = "Notifications";
}
