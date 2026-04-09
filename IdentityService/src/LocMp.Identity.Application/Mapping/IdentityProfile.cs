using AutoMapper;
using LocMp.Identity.Application.DTOs.Role;
using LocMp.Identity.Application.DTOs.User;
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
    }
}