namespace LocMp.Analytics.Application.DTOs;

public sealed record UserGrowthDto(
    DateOnly Date,
    int NewRegistrations,
    int ActiveBuyers,
    int ActiveSellers,
    int BlockedUsers);
