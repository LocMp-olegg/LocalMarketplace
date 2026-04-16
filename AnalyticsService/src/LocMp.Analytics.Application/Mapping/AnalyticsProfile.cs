using AutoMapper;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Domain.Entities;

namespace LocMp.Analytics.Application.Mapping;

public sealed class AnalyticsProfile : Profile
{
    public AnalyticsProfile()
    {
        CreateMap<SellerSalesSummary, SellerSalesSummaryDto>();
        CreateMap<TopProduct, TopProductDto>();
        CreateMap<SellerRatingHistory, SellerRatingHistoryDto>();
        CreateMap<StockAlert, StockAlertDto>();
        CreateMap<ProductViewCounter, ProductViewCounterDto>();
        CreateMap<PlatformDailySummary, PlatformDailySummaryDto>();
        CreateMap<SellerLeaderboard, SellerLeaderboardDto>();
        CreateMap<DisputeSummary, DisputeSummaryDto>();
        CreateMap<GeographicActivity, GeographicActivityDto>();
    }
}