using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class ProductViewCounterConfiguration : IEntityTypeConfiguration<ProductViewCounter>
{
    public void Configure(EntityTypeBuilder<ProductViewCounter> builder)
    {
        builder.ToTable("ProductViewCounters");
        builder.HasKey(x => x.ProductId);

        builder.Property(x => x.LastViewedAt).IsRequired();

        builder.HasIndex(x => x.SellerId);
    }
}
