using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class DeliveryAddressConfiguration : IEntityTypeConfiguration<DeliveryAddress>
{
    public void Configure(EntityTypeBuilder<DeliveryAddress> builder)
    {
        builder.ToTable("DeliveryAddresses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Street).HasMaxLength(200).IsRequired();
        builder.Property(a => a.HouseNumber).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Apartment).HasMaxLength(20);
        builder.Property(a => a.Entrance).HasMaxLength(20);
        builder.Property(a => a.Location).HasColumnType("geography");
        builder.Property(a => a.RecipientName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.RecipientPhone).HasMaxLength(20).IsRequired();

        builder.HasIndex(a => a.OrderId).IsUnique();
    }
}
