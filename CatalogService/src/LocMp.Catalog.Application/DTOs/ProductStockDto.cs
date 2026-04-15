namespace LocMp.Catalog.Application.DTOs;

public sealed record ProductStockDto(
    int StockQuantity,
    bool IsMadeToOrder,
    int? LeadTimeDays
);
