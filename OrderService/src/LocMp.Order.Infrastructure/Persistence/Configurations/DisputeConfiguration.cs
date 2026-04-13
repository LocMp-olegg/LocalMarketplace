using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.ToTable("Disputes");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Reason).HasMaxLength(2000).IsRequired();
        builder.Property(d => d.Resolution).HasMaxLength(2000);
        builder.Property(d => d.CreatedAt).IsRequired();

        builder.HasIndex(d => d.OrderId).IsUnique();
        builder.HasIndex(d => d.Status);
    }
}
