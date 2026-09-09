using Marketplace.Domain.Common;

namespace Marketplace.Domain.Modules.Identity;

/// <summary>
/// Minimal Identity aggregate for this Foundation sprint: enough shape for JWT issuance
/// and RBAC to compile end-to-end. Profile fields (name, addresses, etc.) belong to the
/// Users module and are added when that module's feature sprint starts.
/// </summary>
public class User : AuditableEntity, ISoftDelete, IAggregateRoot
{
    public string PhoneNumber { get; private set; } = default!;
    public string? Email { get; private set; }
    public string PasswordHash { get; private set; } = default!;
    public bool IsPhoneVerified { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { } // EF Core

    public static User Register(string phoneNumber, string passwordHash, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            PhoneNumber = phoneNumber,
            Email = email,
            PasswordHash = passwordHash,
            Status = UserStatus.Active
        };
    }

    public void AssignRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id)) return;
        _userRoles.Add(UserRole.Create(Id, role.Id));
    }

    public void Suspend() => Status = UserStatus.Suspended;
}

public enum UserStatus
{
    Active = 0,
    Suspended = 1
}
