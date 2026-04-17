namespace LocMp.Analytics.Application.DTOs;

public sealed record PlatformDailySummaryDto(
    DateOnly Date,
    int NewRegistrations,
    int ActiveBuyers,
    int ActiveSellers,
    int BlockedUsers,
    int TotalOrders,
    int CompletedOrders,
    int CancelledOrders,
    int DisputedOrders,
    decimal GrossMerchandiseValue,
    decimal AverageOrderValue,
    int NewProducts,
    int NewReviews,
    DateTimeOffset UpdatedAt);
