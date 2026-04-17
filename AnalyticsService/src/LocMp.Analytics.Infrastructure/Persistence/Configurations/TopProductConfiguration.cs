using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class TopProductConfiguration : IEntityTypeConfiguration<TopProduct>
{
    public void Configure(EntityTypeBuilder<TopProduct> builder)
    {
        builder.ToTable("TopProducts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TotalRevenue).HasPrecision(18, 2);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.SellerId, x.ProductId, x.PeriodType, x.PeriodStart }).IsUnique();
        builder.HasIndex(x => x.SellerId);
    }
}
