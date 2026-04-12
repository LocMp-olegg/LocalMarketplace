using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class DisputePhotoConfiguration : IEntityTypeConfiguration<DisputePhoto>
{
    public void Configure(EntityTypeBuilder<DisputePhoto> builder)
    {
        builder.ToTable("DisputePhotos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.StorageUrl).HasMaxLength(2048).IsRequired();
        builder.Property(p => p.ObjectKey).HasMaxLength(512).IsRequired();
        builder.Property(p => p.MimeType).HasMaxLength(50).IsRequired();
        builder.Property(p => p.FileSize).IsRequired();
        builder.Property(p => p.UploadedAt).IsRequired();

        builder.HasOne(p => p.Dispute)
            .WithMany(d => d.Photos)
            .HasForeignKey(p => p.DisputeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.DisputeId);
        builder.HasIndex(p => p.UploadedById);
    }
}
