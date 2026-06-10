namespace LocMp.Identity.Application.DTOs.Courier;

public sealed record CourierProfileDto
{
    public Guid CourierId { get; init; }
    public bool IsActive { get; init; }
    public int ServiceRadiusMeters { get; init; }
    public double? BaseLatitude { get; init; }
    public double? BaseLongitude { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}