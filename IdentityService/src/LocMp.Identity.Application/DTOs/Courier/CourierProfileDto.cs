namespace LocMp.Identity.Application.DTOs.Courier;

public sealed record CourierProfileDto(
    Guid CourierId,
    bool IsActive,
    int ServiceRadiusMeters,
    double? BaseLatitude,
    double? BaseLongitude,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);