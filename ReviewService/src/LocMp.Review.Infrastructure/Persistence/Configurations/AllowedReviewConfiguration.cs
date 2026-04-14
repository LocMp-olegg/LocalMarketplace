using LocMp.Review.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Review.Infrastructure.Persistence.Configurations;

public class AllowedReviewConfiguration : IEntityTypeConfiguration<AllowedReview>
{
    public void Configure(EntityTypeBuilder<AllowedReview> builder)
    {
        builder.ToTable("AllowedReviews");
        builder.HasKey(ar => ar.OrderId);

        builder.Property(ar => ar.AllowedAt).IsRequired();

        builder.HasIndex(ar => ar.BuyerId);
    }
}