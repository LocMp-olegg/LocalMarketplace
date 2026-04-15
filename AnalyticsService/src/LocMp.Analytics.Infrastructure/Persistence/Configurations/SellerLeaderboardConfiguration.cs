using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class SellerLeaderboardConfiguration : IEntityTypeConfiguration<SellerLeaderboard>
{
    public void Configure(EntityTypeBuilder<SellerLeaderboard> builder)
    {
        builder.ToTable("SellerLeaderboards");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SellerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TotalRevenue).HasPrecision(18, 2);
        builder.Property(x => x.AverageRating).HasPrecision(3, 2);
        builder.Property(x => x.UpdatedAt).IsRequired();

        // уникальный продавец в рамках периода
        builder.HasIndex(x => new { x.SellerId, x.PeriodType, x.PeriodStart }).IsUnique();
        // для сортировки leaderboard по рангу
        builder.HasIndex(x => new { x.PeriodType, x.PeriodStart, x.RevenueRank });
    }
}
