using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Application.Mapping;
using LocMp.Order.Infrastructure.Interfaces;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CartEntity = LocMp.Order.Domain.Entities.Cart;

namespace LocMp.Order.Application.Orders.Commands.Cart.AddToCart;

public sealed class AddToCartCommandHandler(
    OrderDbContext db,
    ICatalogClient catalogClient)
    : IRequestHandler<AddToCartCommand, CartDto>
{
    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken ct)
    {
        var product = await catalogClient.GetProductAsync(request.ProductId, ct)
                      ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (!product.IsActive)
            throw new ConflictException("Product is not available.");

        if (!product.IsMadeToOrder && product.StockQuantity < request.Quantity)
            throw new ConflictException($"Insufficient stock. Available: {product.StockQuantity}.");

        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.ExpiresAt > DateTimeOffset.UtcNow, ct);

        if (cart is null)
        {
            cart = new CartEntity(Guid.NewGuid())
            {
                UserId = request.UserId,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
            };
            db.Carts.Add(cart);
        }

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existing is not null)
        {
            if (!product.IsMadeToOrder && product.StockQuantity < existing.Quantity + request.Quantity)
                throw new ConflictException(
                    $"Insufficient stock. Available: {product.StockQuantity}, already in cart: {existing.Quantity}.");

            existing.Quantity += request.Quantity;
            existing.Price = product.Price;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            var item = new CartItem(Guid.NewGuid())
            {
                CartId = cart.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                SellerId = product.SellerId,
                SellerName = product.SellerName ?? string.Empty,
                ShopId = product.ShopId,
                ShopName = product.ShopName,
                Price = product.Price,
                Quantity = request.Quantity
            };
            db.CartItems.Add(item);
        }

        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return CartMapper.ToDto(cart);
    }
}