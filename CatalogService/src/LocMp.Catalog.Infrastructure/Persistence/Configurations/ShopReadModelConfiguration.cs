using LocMp.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Catalog.Infrastructure.Persistence.Configurations;

public class ShopReadModelConfiguration : IEntityTypeConfiguration<ShopReadModel>
{
    public void Configure(EntityTypeBuilder<ShopReadModel> builder)
    {
        builder.ToTable("ShopReadModels");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("ShopId");

        builder.Property(s => s.SellerId).IsRequired();
        builder.Property(s => s.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.WorkingHours).HasMaxLength(200);
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.LastSyncedAt).IsRequired();

        builder.HasIndex(s => s.SellerId);
    }
}
