using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewEntity = LocMp.Review.Domain.Entities.Review;

namespace LocMp.Review.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReviewerName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasMany(r => r.Photos)
            .WithOne(p => p.Review)
            .HasForeignKey(p => p.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Response)
            .WithOne(rr => rr.Review)
            .HasForeignKey<LocMp.Review.Domain.Entities.ReviewResponse>(rr => rr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.OrderId).IsUnique();
        builder.HasIndex(r => new { r.SubjectId, r.SubjectType });
        builder.HasIndex(r => r.ReviewerId);
        builder.HasIndex(r => r.CreatedAt);
    }
}
