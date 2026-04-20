namespace LocMp.Notification.Domain.Enums;

public enum NotificationType
{
    OrderPlaced = 1,
    OrderConfirmed = 2,
    OrderReadyForPickup = 3,
    OrderInDelivery = 4,
    OrderCompleted = 5,
    OrderCancelled = 6,
    OrderDisputed = 7,
    StockDepleted = 8,
    ReviewReceived = 9,
    ReviewReplied = 10,
    SystemAlert = 11
}
