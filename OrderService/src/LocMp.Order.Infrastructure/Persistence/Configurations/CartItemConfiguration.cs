using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.SellerId).IsRequired();
        builder.Property(i => i.SellerName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.ShopId);
        builder.Property(i => i.ShopName).HasMaxLength(200);
        builder.Property(i => i.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
    }
}
