using LocMp.Analytics.Domain.Enums;

namespace LocMp.Analytics.Application.DTOs;

public sealed record SellerSalesSummaryDto(
    PeriodType PeriodType,
    DateOnly PeriodStart,
    decimal TotalRevenue,
    int OrderCount,
    decimal AverageOrderValue,
    int CompletedCount,
    int CancelledCount,
    int DisputedCount,
    DateTimeOffset UpdatedAt);
