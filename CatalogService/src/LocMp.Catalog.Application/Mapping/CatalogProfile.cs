using AutoMapper;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;

namespace LocMp.Catalog.Application.Mapping;

public sealed class CatalogProfile : Profile
{
    public CatalogProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<ProductPhoto, ProductPhotoDto>();
        CreateMap<ShopPhoto, ShopPhotoDto>();

        CreateMap<Shop, ShopDto>()
            .ForMember(d => d.Latitude, o => o.MapFrom(s => s.Location != null ? s.Location.Y : (double?)null))
            .ForMember(d => d.Longitude, o => o.MapFrom(s => s.Location != null ? s.Location.X : (double?)null))
            .ForMember(d => d.Photos, o => o.MapFrom(s => s.Photos.OrderBy(p => p.SortOrder).ToList()));

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Latitude, o => o.MapFrom(s => s.Location != null ? s.Location.Y : (double?)null))
            .ForMember(d => d.Longitude, o => o.MapFrom(s => s.Location != null ? s.Location.X : (double?)null))
            .ForMember(d => d.ShopName, o => o.MapFrom(s => s.Shop != null ? s.Shop.BusinessName : null))
            .ForMember(d => d.SellerName, o => o.Ignore())
            .ForMember(d => d.MainPhotoUrl, o => o.MapFrom(s =>
                s.Photos.FirstOrDefault(ph => ph.IsMain)!.StorageUrl
                ?? s.Photos.OrderBy(ph => ph.SortOrder).FirstOrDefault()!.StorageUrl))
            .ForMember(d => d.Photos, o => o.MapFrom(s => s.Photos.OrderBy(ph => ph.SortOrder).ToList()))
            .ForMember(d => d.Tags, o => o.MapFrom(s => s.ProductTags.Select(pt => pt.Tag.Name).ToList()));
    }
}