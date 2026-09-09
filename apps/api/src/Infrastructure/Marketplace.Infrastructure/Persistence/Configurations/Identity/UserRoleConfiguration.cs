using Marketplace.Domain.Modules.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Infrastructure.Persistence.Configurations.Identity;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
        builder.HasIndex(ur => ur.RoleId);

        builder.HasOne<Role>().WithMany().HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
