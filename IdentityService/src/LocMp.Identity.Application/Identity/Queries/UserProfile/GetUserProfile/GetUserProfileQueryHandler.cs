using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.UserProfile.GetUserProfile;

public sealed class GetUserProfileQueryHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var profile = await userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserProfileDto(
                u.Id,
                u.UserName!,
                u.Email!,
                u.FirstName,
                u.LastName,
                u.Gender.HasValue ? (Gender)u.Gender.Value : null,
                u.BirthDate,
                u.PhoneNumber,
                u.RegisteredAt,
                u.Photo != null,
                u.Photo != null ? u.Photo.MimeType : null,
                u.Photo != null ? u.Photo.UploadedAt.Ticks : null
            ))
            .FirstOrDefaultAsync(ct);

        return profile ?? throw new KeyNotFoundException($"User with id '{request.UserId}' was not found");
    }
}