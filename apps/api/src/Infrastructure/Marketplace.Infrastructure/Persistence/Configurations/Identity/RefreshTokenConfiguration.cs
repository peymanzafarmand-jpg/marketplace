using Marketplace.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Infrastructure.Persistence.Configurations.Identity;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash).HasMaxLength(500).IsRequired();
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => rt.UserId);

        builder.Property(rt => rt.DeviceId).HasMaxLength(200);
        builder.Property(rt => rt.UserAgent).HasMaxLength(500);

        // Supports the future "list/revoke sessions for this device" and max-5-concurrent
        // -sessions enforcement (deferred to the Identity feature sprint — see entity doc comment).
        builder.HasIndex(rt => new { rt.UserId, rt.DeviceId });
    }
}
