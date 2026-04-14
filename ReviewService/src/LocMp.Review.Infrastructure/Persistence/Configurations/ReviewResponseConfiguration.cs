using LocMp.Review.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Review.Infrastructure.Persistence.Configurations;

public class ReviewResponseConfiguration : IEntityTypeConfiguration<ReviewResponse>
{
    public void Configure(EntityTypeBuilder<ReviewResponse> builder)
    {
        builder.ToTable("ReviewResponses");
        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.Comment).HasMaxLength(2000).IsRequired();
        builder.Property(rr => rr.CreatedAt).IsRequired();

        builder.HasIndex(rr => rr.ReviewId).IsUnique();
    }
}
