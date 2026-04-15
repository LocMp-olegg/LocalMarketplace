using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class StockAlertConfiguration : IEntityTypeConfiguration<StockAlert>
{
    public void Configure(EntityTypeBuilder<StockAlert> builder)
    {
        builder.ToTable("StockAlerts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.AlertType).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.SellerId);
        builder.HasIndex(x => new { x.SellerId, x.IsAcknowledged });
        builder.HasIndex(x => new { x.ProductId, x.AlertType });
    }
}
