using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class SellerRatingHistoryConfiguration : IEntityTypeConfiguration<SellerRatingHistory>
{
    public void Configure(EntityTypeBuilder<SellerRatingHistory> builder)
    {
        builder.ToTable("SellerRatingHistory");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AverageRating).HasPrecision(3, 2);

        builder.HasIndex(x => new { x.SellerId, x.RecordedAt }).IsUnique();
        builder.HasIndex(x => x.SellerId);
    }
}
