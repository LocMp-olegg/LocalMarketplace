using LocMp.Review.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Review.Infrastructure.Persistence.Configurations;

public class ReviewPhotoConfiguration : IEntityTypeConfiguration<ReviewPhoto>
{
    public void Configure(EntityTypeBuilder<ReviewPhoto> builder)
    {
        builder.ToTable("ReviewPhotos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.StorageUrl).HasMaxLength(1000).IsRequired();
        builder.Property(p => p.ObjectKey).HasMaxLength(500).IsRequired();
        builder.Property(p => p.MimeType).HasMaxLength(100).IsRequired();
        builder.Property(p => p.UploadedAt).IsRequired();

        builder.HasIndex(p => p.ReviewId);
    }
}
