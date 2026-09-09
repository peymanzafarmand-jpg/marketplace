namespace Marketplace.Domain.Modules.Users;

/// <summary>
/// Assembly/namespace anchor for the Users module. No business entities yet — this Sprint
/// (1.1 Backend Foundation) only wires up cross-cutting infrastructure. Entities, value
/// objects and domain services for Users are added when its first feature Sprint starts.
/// Keeping every module in its own namespace from day one is what makes future extraction
/// into a separate service/assembly a namespace move rather than a redesign.
/// </summary>
public static class UsersModuleMarker
{
    public const string ModuleName = "Users";
}
