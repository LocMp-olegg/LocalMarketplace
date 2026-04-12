using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderEntity = LocMp.Order.Domain.Entities.Order;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(o => o.BuyerComment).HasMaxLength(1000);
        builder.Property(o => o.CreatedAt).IsRequired();

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Photos)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.DeliveryAddress)
            .WithOne(a => a.Order)
            .HasForeignKey<DeliveryAddress>(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.CourierAssignment)
            .WithOne(ca => ca.Order)
            .HasForeignKey<CourierAssignment>(ca => ca.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Dispute)
            .WithOne(d => d.Order)
            .HasForeignKey<Dispute>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.BuyerId);
        builder.HasIndex(o => o.SellerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
    }
}
