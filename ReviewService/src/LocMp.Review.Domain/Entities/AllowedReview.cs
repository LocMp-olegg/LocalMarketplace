namespace LocMp.Review.Domain.Entities;

/// <summary>
/// Запись, созданная OrderCompletedConsumer, разрешающая покупателю написать отзывы по заказу.
/// Хранит всех возможных субъектов для оценки: продавца, курьера и список товаров.
/// </summary>
public class AllowedReview
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? CourierId { get; set; }
    public List<Guid> ProductIds { get; set; } = [];
    public DateTimeOffset AllowedAt { get; set; } = DateTimeOffset.UtcNow;
}