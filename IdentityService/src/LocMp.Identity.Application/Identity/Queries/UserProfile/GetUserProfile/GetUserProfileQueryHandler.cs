using LocMp.BuildingBlocks.Application.Exceptions;
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
        var user = await userManager.Users
                       .AsNoTracking()
                       .Include(u => u.Photo)
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                   ?? throw new NotFoundException($"User with id '{request.UserId}' was not found");

        var roles = await userManager.GetRolesAsync(user);

        return new UserProfileDto(
            user.Id,
            user.UserName!,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Gender.HasValue ? (Gender)user.Gender.Value : null,
            user.BirthDate,
            user.PhoneNumber,
            user.RegisteredAt,
            user.Photo != null,
            user.Photo?.MimeType,
            user.Photo?.UploadedAt.Ticks,
            [.. roles]
        );
    }
}