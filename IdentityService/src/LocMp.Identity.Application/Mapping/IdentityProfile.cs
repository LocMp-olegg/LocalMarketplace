using AutoMapper;
using LocMp.Identity.Application.DTOs.Courier;
using LocMp.Identity.Application.DTOs.Role;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Application.DTOs.UserAddress;
using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;

namespace LocMp.Identity.Application.Mapping;

public sealed class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationRole, RoleDto>();

        CreateMap<ApplicationUser, UserProfileDto>()
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.HasValue ? (Gender?)s.Gender.Value : null))
            .ForMember(d => d.HasPhoto, o => o.MapFrom(s => s.Photo != null))
            .ForMember(d => d.PhotoMimeType, o => o.MapFrom(s => s.Photo != null ? s.Photo.MimeType : null))
            .ForMember(d => d.PhotoVersion,
                o => o.MapFrom(s => s.Photo != null ? (long?)s.Photo.UploadedAt.Ticks : null))
            .ForMember(d => d.Roles, o => o.Ignore());

        CreateMap<UserAddress, UserAddressDto>()
            .ForMember(d => d.Latitude, o => o.MapFrom(s => s.Location != null ? (double?)s.Location.Y : null))
            .ForMember(d => d.Longitude, o => o.MapFrom(s => s.Location != null ? (double?)s.Location.X : null));

        CreateMap<CourierProfile, CourierProfileDto>()
            .ForMember(d => d.BaseLatitude, o => o.MapFrom(s => s.BaseLocation != null ? (double?)s.BaseLocation.Y : null))
            .ForMember(d => d.BaseLongitude, o => o.MapFrom(s => s.BaseLocation != null ? (double?)s.BaseLocation.X : null));
    }
}