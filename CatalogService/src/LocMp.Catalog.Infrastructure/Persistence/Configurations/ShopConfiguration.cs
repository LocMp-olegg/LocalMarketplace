using LocMp.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Catalog.Infrastructure.Persistence.Configurations;

public class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.ToTable("Shops");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SellerId).IsRequired();
        builder.Property(s => s.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.Inn).HasMaxLength(12);
        builder.Property(s => s.BusinessType).IsRequired();
        builder.Property(s => s.WorkingHours).HasMaxLength(200);
        builder.Property(s => s.Location).HasColumnType("geography");
        builder.Property(s => s.AvatarUrl).HasMaxLength(1024);
        builder.Property(s => s.AvatarObjectKey).HasMaxLength(512);
        builder.Property(s => s.AllowCourierDelivery).HasDefaultValue(true);
        builder.Property(s => s.MaxCourierDistanceMeters);
        builder.OwnsOne(s => s.Address, a =>
        {
            a.Property(x => x.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            a.Property(x => x.Street).HasColumnName("Street").HasMaxLength(250).IsRequired();
            a.Property(x => x.HouseNumber).HasColumnName("HouseNumber").HasMaxLength(20).IsRequired();
            a.Property(x => x.Apartment).HasColumnName("Apartment").HasMaxLength(20);
            a.Property(x => x.Entrance).HasColumnName("Entrance").HasMaxLength(10);
            a.Property(x => x.Floor).HasColumnName("Floor").HasMaxLength(10);
        });

        builder.Property(s => s.AverageRating).HasPrecision(4, 2).HasDefaultValue(0m);
        builder.Property(s => s.ReviewCount).HasDefaultValue(0);

        builder.Property(s => s.IsVerified).HasDefaultValue(true);
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => s.SellerId);
    }
}
