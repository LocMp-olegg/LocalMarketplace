namespace LocMp.Analytics.Application.DTOs;

public sealed record ProductViewCounterDto(
    Guid ProductId,
    int TotalViews,
    int ViewsToday,
    int ViewsThisWeek,
    DateTimeOffset LastViewedAt);
