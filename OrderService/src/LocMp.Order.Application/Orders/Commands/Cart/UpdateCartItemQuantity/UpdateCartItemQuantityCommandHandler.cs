using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Interfaces;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Cart.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandHandler(
    OrderDbContext db,
    ICatalogClient catalogClient,
    IMapper mapper)
    : IRequestHandler<UpdateCartItemQuantityCommand, CartDto>
{
    public async Task<CartDto> Handle(UpdateCartItemQuantityCommand request, CancellationToken ct)
    {
        var item = await db.CartItems
            .Include(i => i.Cart)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(i => i.Id == request.CartItemId, ct)
            ?? throw new NotFoundException($"Cart item '{request.CartItemId}' not found.");

        if (item.Cart.UserId != request.UserId)
            throw new ForbiddenException("You can only update items in your own cart.");

        if (item.Cart.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new ConflictException("Cart has expired.");

        var product = await catalogClient.GetProductAsync(item.ProductId, ct)
                      ?? throw new NotFoundException($"Product '{item.ProductId}' not found.");

        if (product.StockQuantity < request.Quantity)
            throw new ConflictException($"Insufficient stock. Available: {product.StockQuantity}.");

        item.Quantity = request.Quantity;
        item.Price = product.Price;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.Cart.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return mapper.Map<CartDto>(item.Cart);
    }
}
