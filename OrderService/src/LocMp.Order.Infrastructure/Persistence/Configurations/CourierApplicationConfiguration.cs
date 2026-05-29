using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class CourierApplicationConfiguration : IEntityTypeConfiguration<CourierApplication>
{
    public void Configure(EntityTypeBuilder<CourierApplication> builder)
    {
        builder.ToTable("CourierApplications");
        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.CourierName).HasMaxLength(200).IsRequired();
        builder.Property(ca => ca.CourierPhone).HasMaxLength(20).IsRequired();
        builder.Property(ca => ca.CourierLocation).HasColumnType("geography");
        builder.Property(ca => ca.AppliedAt).IsRequired();

        builder.HasIndex(ca => ca.OrderId);
        builder.HasIndex(ca => ca.CourierId);
        builder.HasIndex(ca => new { ca.OrderId, ca.CourierId }).IsUnique();
    }
}