using LocMp.Analytics.Domain.Enums;

namespace LocMp.Analytics.Application.DTOs;

public sealed record StockAlertDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int CurrentStock,
    StockAlertType AlertType,
    bool IsAcknowledged,
    DateTimeOffset? AcknowledgedAt,
    DateTimeOffset CreatedAt);
