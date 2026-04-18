using AutoMapper;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Entities;
using OrderEntity = LocMp.Order.Domain.Entities.Order;

namespace LocMp.Order.Application.Mapping;

public sealed class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderItem, OrderItemDto>();

        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();

        CreateMap<OrderPhoto, OrderPhotoDto>();

        CreateMap<DeliveryAddress, DeliveryAddressDto>()
            .ForCtorParam(nameof(DeliveryAddressDto.Latitude),
                o => o.MapFrom(s => s.Location != null ? s.Location.Y : (double?)null))
            .ForCtorParam(nameof(DeliveryAddressDto.Longitude),
                o => o.MapFrom(s => s.Location != null ? s.Location.X : (double?)null));

        CreateMap<CourierAssignment, CourierAssignmentDto>();

        CreateMap<DisputePhoto, DisputePhotoDto>();

        CreateMap<Dispute, DisputeDto>()
            .ForCtorParam(nameof(DisputeDto.Photos), o => o.MapFrom(s => s.Photos.OrderBy(p => p.SortOrder).ToList()));

        CreateMap<OrderEntity, OrderDto>()
            .ForCtorParam(nameof(OrderDto.Items), o => o.MapFrom(s => s.Items.ToList()))
            .ForCtorParam(nameof(OrderDto.StatusHistory),
                o => o.MapFrom(s => s.StatusHistory.OrderBy(h => h.ChangedAt).ToList()))
            .ForCtorParam(nameof(OrderDto.Photos), o => o.MapFrom(s => s.Photos.OrderBy(p => p.SortOrder).ToList()))
            .ForCtorParam(nameof(OrderDto.SellerName), o => o.MapFrom(s => s.SellerName))
            .ForCtorParam(nameof(OrderDto.CheckoutId), o => o.MapFrom(s => s.CheckoutId))
            .ForCtorParam(nameof(OrderDto.ShopId), o => o.MapFrom(s => s.ShopId))
            .ForCtorParam(nameof(OrderDto.ShopName), o => o.MapFrom(s => s.ShopName));
    }
}