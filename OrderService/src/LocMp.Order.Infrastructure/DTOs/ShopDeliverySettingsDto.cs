namespace LocMp.Order.Infrastructure.DTOs;

public sealed record ShopDeliverySettingsDto(
    bool AllowCourierDelivery,
    int? MaxCourierDistanceMeters,
    double? Latitude,
    double? Longitude);
