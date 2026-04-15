using LocMp.Contracts.Orders;

namespace LocMp.Order.Api.Requests;

public sealed record OpenDisputeRequest(DisputeType DisputeType, string Reason);
