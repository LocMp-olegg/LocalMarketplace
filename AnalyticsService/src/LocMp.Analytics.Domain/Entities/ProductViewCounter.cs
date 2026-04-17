namespace LocMp.Analytics.Domain.Entities;

public class ProductViewCounter
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public int TotalViews { get; set; }
    public int ViewsToday { get; set; }
    public int ViewsThisWeek { get; set; }
    public DateTimeOffset LastViewedAt { get; set; }
}
