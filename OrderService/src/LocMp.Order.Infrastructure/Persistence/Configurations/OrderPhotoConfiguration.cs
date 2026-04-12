using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class OrderPhotoConfiguration : IEntityTypeConfiguration<OrderPhoto>
{
    public void Configure(EntityTypeBuilder<OrderPhoto> builder)
    {
        builder.ToTable("OrderPhotos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.StorageUrl).HasMaxLength(2048).IsRequired();
        builder.Property(p => p.ObjectKey).HasMaxLength(512).IsRequired();
        builder.Property(p => p.MimeType).HasMaxLength(50).IsRequired();
        builder.Property(p => p.FileSize).IsRequired();
        builder.Property(p => p.UploadedAt).IsRequired();

        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.UploadedById);
    }
}
