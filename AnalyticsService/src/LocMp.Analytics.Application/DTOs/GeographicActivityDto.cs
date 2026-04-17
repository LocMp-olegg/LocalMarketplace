using LocMp.Analytics.Domain.Enums;

namespace LocMp.Analytics.Application.DTOs;

public sealed record GeographicActivityDto(
    Guid ComplexId,
    string ComplexName,
    PeriodType PeriodType,
    DateOnly PeriodStart,
    int ActiveSellers,
    int ActiveBuyers,
    int TotalOrders,
    decimal TotalRevenue);
