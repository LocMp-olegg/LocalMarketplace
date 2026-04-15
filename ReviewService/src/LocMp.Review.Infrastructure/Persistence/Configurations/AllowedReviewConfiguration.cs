using System.Text.Json;
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

        builder.Property(ar => ar.ProductIds)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>());

        builder.HasIndex(ar => ar.BuyerId);
    }
}