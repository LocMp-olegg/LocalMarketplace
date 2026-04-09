using AutoMapper;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;

namespace LocMp.Catalog.Application.Mapping;

public sealed class CatalogProfile : Profile
{
    public CatalogProfile()
    {
        CreateMap<Shop, ShopDto>()
            .ConstructUsing((s, _) => new ShopDto(
                s.Id,
                s.SellerId,
                s.BusinessName,
                s.PhoneNumber,
                s.Email,
                s.Description,
                s.Inn,
                s.BusinessType,
                s.WorkingHours,
                s.ServiceRadiusMeters,
                s.Location != null ? s.Location.Y : null,
                s.Location != null ? s.Location.X : null,
                s.AvatarUrl,
                s.IsVerified,
                s.IsActive,
                s.CreatedAt,
                s.UpdatedAt))
            .ForAllMembers(o => o.Ignore());
    }
}
