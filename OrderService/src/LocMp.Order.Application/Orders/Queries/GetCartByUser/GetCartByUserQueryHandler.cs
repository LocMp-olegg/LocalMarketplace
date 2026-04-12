using AutoMapper;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.GetCartByUser;

public sealed class GetCartByUserQueryHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetCartByUserQuery, CartDto?>
{
    public async Task<CartDto?> Handle(GetCartByUserQuery request, CancellationToken ct)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.ExpiresAt > DateTimeOffset.UtcNow, ct);

        return cart is null ? null : mapper.Map<CartDto>(cart);
    }
}
