using LocMp.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Catalog.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(4000);
        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.Unit).HasMaxLength(20).IsRequired();
        builder.Property(p => p.StockQuantity).IsRequired();
        builder.Property(p => p.IsMadeToOrder).HasDefaultValue(false);
        builder.Property(p => p.LeadTimeDays);
        builder.Property(p => p.Attributes).HasColumnType("jsonb");
        builder.Property(p => p.Location).HasColumnType("geography");
        builder.Property(p => p.AverageRating).HasPrecision(3, 2).HasDefaultValue(0m);
        builder.Property(p => p.ReviewCount).HasDefaultValue(0);

        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasOne(p => p.Shop)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.ShopId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasIndex(p => p.SellerId);
        builder.HasIndex(p => p.ShopId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.IsDeleted);
    }
}