using LocMp.Contracts.Orders;

namespace LocMp.Order.Api.Requests;

public sealed record ResolveDisputeRequest(DisputeOutcome Outcome, string Resolution);