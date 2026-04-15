using LocMp.Contracts.Orders;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Infrastructure.Consumers;

public sealed class DisputeResolvedConsumer(ReviewDbContext db) : IConsumer<DisputeResolvedEvent>
{
    public async Task Consume(ConsumeContext<DisputeResolvedEvent> context)
    {
        var msg = context.Message;

        var existing = await db.AllowedReviews
            .FirstOrDefaultAsync(ar => ar.OrderId == msg.OrderId, context.CancellationToken);

        if (existing is not null)
            return;

        var includeProducts = msg.Outcome == DisputeOutcome.SellerFavored
                              || msg.DisputeType == DisputeType.DefectiveItem;

        var includeCourier = msg.CourierId.HasValue
                             && (msg.Outcome == DisputeOutcome.SellerFavored
                                 || msg.DisputeType == DisputeType.NotDelivered
                                 || msg.DisputeType == DisputeType.CourierIssue);

        db.AllowedReviews.Add(new AllowedReview
        {
            OrderId = msg.OrderId,
            BuyerId = msg.BuyerId,
            SellerId = msg.SellerId,
            CourierId = includeCourier ? msg.CourierId : null,
            ProductIds = includeProducts ? msg.ProductIds.ToList() : [],
            AllowedAt = msg.OccurredAt
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}