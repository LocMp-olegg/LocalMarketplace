namespace LocMp.Order.Infrastructure.Options;

public sealed class DisputeOptions
{
    /// <summary>Number of days after which an unresolved dispute is auto-closed.</summary>
    public int AutoResolveDays { get; set; } = 14;
}
