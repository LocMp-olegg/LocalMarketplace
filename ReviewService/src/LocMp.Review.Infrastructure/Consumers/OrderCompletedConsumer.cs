using LocMp.Contracts.Orders;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Infrastructure.Consumers;

public sealed class OrderCompletedConsumer(ReviewDbContext db) : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var msg = context.Message;

        var existing = await db.AllowedReviews
            .FirstOrDefaultAsync(ar => ar.OrderId == msg.OrderId, context.CancellationToken);

        if (existing is not null)
            return; // idempotent — already unlocked

        db.AllowedReviews.Add(new AllowedReview
        {
            OrderId = msg.OrderId,
            BuyerId = msg.BuyerId,
            SellerId = msg.SellerId,
            CourierId = msg.CourierId,
            AllowedAt = msg.OccurredAt
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
