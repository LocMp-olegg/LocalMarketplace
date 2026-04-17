using LocMp.Analytics.Domain.Enums;
using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class StockAlert(Guid id) : Entity<Guid>(id)
{
    public Guid SellerId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!; // snapshot
    public int CurrentStock { get; set; }
    public StockAlertType AlertType { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
