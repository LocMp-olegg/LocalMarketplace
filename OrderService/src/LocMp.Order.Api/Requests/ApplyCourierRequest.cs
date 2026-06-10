namespace LocMp.Order.Api.Requests;

public sealed record ApplyCourierRequest(
    string CourierName,
    string CourierPhone,
    double? Latitude,
    double? Longitude);