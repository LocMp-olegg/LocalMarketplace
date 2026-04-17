using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class ProductRatingSummaryConfiguration : IEntityTypeConfiguration<ProductRatingSummary>
{
    public void Configure(EntityTypeBuilder<ProductRatingSummary> builder)
    {
        builder.ToTable("ProductRatingSummaries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.AverageRating).HasPrecision(3, 2);

        builder.HasIndex(x => x.ProductId).IsUnique();
        builder.HasIndex(x => x.SellerId);
    }
}
