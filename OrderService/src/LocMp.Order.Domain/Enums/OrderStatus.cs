namespace LocMp.Order.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    ReadyForPickup = 3,
    InDelivery = 4,
    Completed = 5,
    Cancelled = 6,
    Disputed = 7
}