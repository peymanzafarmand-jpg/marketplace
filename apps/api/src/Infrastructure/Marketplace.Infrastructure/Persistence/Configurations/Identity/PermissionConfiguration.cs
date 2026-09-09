using Marketplace.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Infrastructure.Persistence.Configurations.Identity;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(150).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}
