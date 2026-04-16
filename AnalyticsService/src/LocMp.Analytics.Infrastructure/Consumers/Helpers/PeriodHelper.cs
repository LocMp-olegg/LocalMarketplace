using LocMp.Analytics.Domain.Enums;

namespace LocMp.Analytics.Infrastructure.Consumers.Helpers;

internal static class PeriodHelper
{
    public static IEnumerable<PeriodType> All => Enum.GetValues<PeriodType>();

    public static DateOnly GetPeriodStart(PeriodType periodType, DateTimeOffset now) =>
        periodType switch
        {
            PeriodType.Daily   => DateOnly.FromDateTime(now.UtcDateTime),
            PeriodType.Weekly  => DateOnly.FromDateTime(now.UtcDateTime.AddDays(-(((int)now.UtcDateTime.DayOfWeek + 6) % 7))),
            PeriodType.Monthly => new DateOnly(now.Year, now.Month, 1),
            _                  => throw new ArgumentOutOfRangeException(nameof(periodType))
        };
}
