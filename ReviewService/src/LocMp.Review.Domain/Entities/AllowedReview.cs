namespace LocMp.Review.Domain.Entities;

/// <summary>
/// Запись, созданная OrderCompletedConsumer, разрешающая покупателю написать отзыв по заказу.
/// </summary>
public class AllowedReview
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? CourierId { get; set; }
    public DateTimeOffset AllowedAt { get; set; } = DateTimeOffset.UtcNow;
}