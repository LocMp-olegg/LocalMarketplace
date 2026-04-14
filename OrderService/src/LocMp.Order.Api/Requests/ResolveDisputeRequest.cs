using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Api.Requests;

public sealed record ResolveDisputeRequest(DisputeOutcome Outcome, string Resolution);