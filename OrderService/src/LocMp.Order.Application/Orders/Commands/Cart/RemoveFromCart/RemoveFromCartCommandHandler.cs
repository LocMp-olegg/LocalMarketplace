using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Cart.RemoveFromCart;

public sealed class RemoveFromCartCommandHandler(OrderDbContext db)
    : IRequestHandler<RemoveFromCartCommand>
{
    public async Task Handle(RemoveFromCartCommand request, CancellationToken ct)
    {
        var item = await db.CartItems
                       .Include(i => i.Cart)
                       .FirstOrDefaultAsync(i => i.Id == request.CartItemId, ct)
                   ?? throw new NotFoundException($"Cart item '{request.CartItemId}' not found.");

        if (item.Cart.UserId != request.UserId)
            throw new ForbiddenException("You can only remove items from your own cart.");

        db.CartItems.Remove(item);

        item.Cart.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}