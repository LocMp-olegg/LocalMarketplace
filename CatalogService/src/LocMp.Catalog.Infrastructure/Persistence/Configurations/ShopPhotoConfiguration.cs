using LocMp.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Catalog.Infrastructure.Persistence.Configurations;

public class ShopPhotoConfiguration : IEntityTypeConfiguration<ShopPhoto>
{
    public void Configure(EntityTypeBuilder<ShopPhoto> builder)
    {
        builder.ToTable("ShopPhotos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.StorageUrl).HasMaxLength(1024).IsRequired();
        builder.Property(p => p.ObjectKey).HasMaxLength(512).IsRequired();
        builder.Property(p => p.MimeType).HasMaxLength(50).IsRequired();
        builder.Property(p => p.FileSize).IsRequired();
        builder.Property(p => p.SortOrder).HasDefaultValue(0);
        builder.Property(p => p.UploadedAt).IsRequired();

        builder.HasOne(p => p.Shop)
            .WithMany(s => s.Photos)
            .HasForeignKey(p => p.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ShopId);
    }
}
