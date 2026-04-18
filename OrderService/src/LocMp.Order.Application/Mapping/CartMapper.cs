using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Entities;

namespace LocMp.Order.Application.Mapping;

public static class CartMapper
{
    public static CartDto ToDto(Cart cart)
    {
        var groups = cart.Items
            .GroupBy(i => (i.SellerId, i.ShopId, i.SellerName, i.ShopName))
            .Select(g =>
            {
                var items = g.Select(i => new CartItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.SellerId,
                    i.SellerName,
                    i.ShopId,
                    i.ShopName,
                    i.Price,
                    i.Quantity,
                    i.Price * i.Quantity)).ToList();

                return new CartGroupDto(
                    g.Key.SellerId,
                    g.Key.SellerName,
                    g.Key.ShopId,
                    g.Key.ShopName,
                    items,
                    items.Sum(i => i.Subtotal));
            })
            .ToList();

        return new CartDto(
            cart.Id,
            cart.UserId,
            cart.CreatedAt,
            cart.ExpiresAt,
            groups,
            groups.Sum(g => g.GroupTotal));
    }
}
