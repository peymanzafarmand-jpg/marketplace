using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Infrastructure.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).HasMaxLength(300).IsRequired();
        builder.Property(m => m.Content).HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2000);

        // Polled by the background processor: WHERE processed_on IS NULL ORDER BY occurred_on
        builder.HasIndex(m => m.ProcessedOn);
    }
}
