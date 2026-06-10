namespace LocMp.Order.Infrastructure.DTOs;

public sealed record ShopDeliverySettingsDto(
    bool AllowCourierDelivery,
    bool AllowSellerDelivery,
    int? ServiceRadiusMeters,
    int? MaxCourierDistanceMeters,
    double? Latitude,
    double? Longitude);
