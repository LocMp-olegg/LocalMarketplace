namespace LocMp.Order.Api.Requests;

public sealed record AddToCartRequest(Guid ProductId, int Quantity);
