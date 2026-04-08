using LocMp.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Catalog.Infrastructure.Persistence.Configurations;

public class ProductPhotoConfiguration : IEntityTypeConfiguration<ProductPhoto>
{
    public void Configure(EntityTypeBuilder<ProductPhoto> builder)
    {
        builder.ToTable("ProductPhotos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.StorageUrl).HasMaxLength(1024).IsRequired();
        builder.Property(p => p.ObjectKey).HasMaxLength(512).IsRequired();
        builder.Property(p => p.MimeType).HasMaxLength(50).IsRequired();
        builder.Property(p => p.FileSize).IsRequired();
        builder.Property(p => p.SortOrder).HasDefaultValue(0);
        builder.Property(p => p.IsMain).HasDefaultValue(false);
        builder.Property(p => p.UploadedAt).IsRequired();

        builder.HasOne(p => p.Product)
            .WithMany(p => p.Photos)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.ProductId, p.IsMain });
    }
}