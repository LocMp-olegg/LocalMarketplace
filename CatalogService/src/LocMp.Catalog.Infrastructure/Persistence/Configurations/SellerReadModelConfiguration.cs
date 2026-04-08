using LocMp.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Catalog.Infrastructure.Persistence.Configurations;

public class SellerReadModelConfiguration : IEntityTypeConfiguration<SellerReadModel>
{
    public void Configure(EntityTypeBuilder<SellerReadModel> builder)
    {
        builder.ToTable("SellerReadModels");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("SellerId");

        builder.Property(s => s.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.AvatarUrl).HasMaxLength(1024);
        builder.Property(s => s.AverageRating).HasPrecision(3, 2).HasDefaultValue(0m);
        builder.Property(s => s.ReviewCount).HasDefaultValue(0);
        builder.Property(s => s.Location).HasColumnType("geometry(Point, 4326)");
        builder.Property(s => s.LastSyncedAt).IsRequired();
    }
}