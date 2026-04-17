using LocMp.Contracts.Orders;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Domain.Enums;
using LocMp.Review.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Infrastructure.Consumers;

public sealed class OrderCompletedConsumer(ReviewDbContext db) : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var msg = context.Message;

        if (await db.AllowedReviews.AnyAsync(ar => ar.OrderId == msg.OrderId, context.CancellationToken))
            return;

        // Subjects this buyer has already reviewed globally
        var reviewed = (await db.Reviews
                .Where(r => r.ReviewerId == msg.BuyerId)
                .Select(r => new { r.SubjectType, r.SubjectId })
                .ToListAsync(context.CancellationToken))
            .Select(r => (r.SubjectType, r.SubjectId))
            .ToHashSet();

        var sellerId   = reviewed.Contains((ReviewSubjectType.Seller, msg.SellerId)) ? (Guid?)null : msg.SellerId;
        var courierId  = msg.CourierId.HasValue && reviewed.Contains((ReviewSubjectType.Courier, msg.CourierId.Value))
                         ? null : msg.CourierId;
        var productIds = msg.Products
            .Select(p => p.ProductId)
            .Where(id => !reviewed.Contains((ReviewSubjectType.Product, id)))
            .ToList();

        if (sellerId is null && courierId is null && productIds.Count == 0)
            return;

        db.AllowedReviews.Add(new AllowedReview
        {
            OrderId    = msg.OrderId,
            BuyerId    = msg.BuyerId,
            SellerId   = sellerId ?? Guid.Empty,
            CourierId  = courierId,
            ProductIds = productIds,
            AllowedAt  = msg.OccurredAt
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
