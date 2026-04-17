namespace LocMp.Order.Infrastructure.DTOs;

public sealed record ProductSnapshotDto(
    Guid Id,
    Guid SellerId,
    string? SellerName,
    Guid? ShopId,
    string? ShopName,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? MainPhotoUrl,
    bool IsActive,
    bool IsMadeToOrder = false,
    int? LeadTimeDays = null);