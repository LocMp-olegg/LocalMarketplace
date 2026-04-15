namespace LocMp.Contracts.Orders;

public enum DisputeOutcome
{
    BuyerFavored = 1, // заказ отменяется, сток возвращается продавцу
    SellerFavored = 2 // заказ засчитывается как выполненный, сток остается у покупателя
}