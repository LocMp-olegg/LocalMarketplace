namespace LocMp.Order.Api.Requests;

public sealed record AssignCourierRequest(
    Guid CourierId,
    string CourierName,
    string CourierPhone);
