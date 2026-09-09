using Marketplace.Domain.Common;

namespace Marketplace.Domain.Modules.Identity;

/// <summary>
/// Refresh token record supporting rotation + theft detection (ADR-008 in the architecture
/// doc): only the hash is stored, never the raw token; ReplacedByTokenId links a rotation
/// chain so re-use of a revoked token can be detected and the whole chain revoked.
///
/// DeviceId/UserAgent capture which device/client issued this token (device binding) — this
/// sprint only adds the schema; the actual max-5-concurrent-sessions enforcement is deferred
/// to the Identity feature sprint (Task 1.5 item 5), once a real login endpoint exists to
/// populate/enforce it meaningfully.
/// </summary>
public class RefreshToken : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>Client-supplied device identifier (e.g. installation id from mobile app), nullable — not every client sends one yet.</summary>
    public string? DeviceId { get; private set; }

    /// <summary>Raw User-Agent header captured at issuance, for session-list display/audit purposes.</summary>
    public string? UserAgent { get; private set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

    private RefreshToken() { } // EF Core

    public static RefreshToken Issue(
        Guid userId, string tokenHash, DateTimeOffset expiresAt, string? deviceId = null, string? userAgent = null) =>
        new() { UserId = userId, TokenHash = tokenHash, ExpiresAt = expiresAt, DeviceId = deviceId, UserAgent = userAgent };

    public void Revoke(Guid? replacedByTokenId = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
