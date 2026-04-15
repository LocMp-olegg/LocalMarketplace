using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class SellerSalesSummaryConfiguration : IEntityTypeConfiguration<SellerSalesSummary>
{
    public void Configure(EntityTypeBuilder<SellerSalesSummary> builder)
    {
        builder.ToTable("SellerSalesSummaries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalRevenue).HasPrecision(18, 2);
        builder.Property(x => x.AverageOrderValue).HasPrecision(18, 2);
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Продавец + период = уникальная запись
        builder.HasIndex(x => new { x.SellerId, x.PeriodType, x.PeriodStart }).IsUnique();
        builder.HasIndex(x => x.SellerId);
    }
}
