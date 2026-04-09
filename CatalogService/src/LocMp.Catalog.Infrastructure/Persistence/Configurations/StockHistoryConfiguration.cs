using LocMp.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Catalog.Infrastructure.Persistence.Configurations;

public class StockHistoryConfiguration : IEntityTypeConfiguration<StockHistory>
{
    public void Configure(EntityTypeBuilder<StockHistory> builder)
    {
        builder.ToTable("StockHistory");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ChangeType).IsRequired();
        builder.Property(s => s.QuantityDelta).IsRequired();
        builder.Property(s => s.QuantityAfter).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasOne(s => s.Product)
            .WithMany(p => p.StockHistory)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(s => !s.Product.IsDeleted);

        builder.HasIndex(s => s.ProductId);
    }
}