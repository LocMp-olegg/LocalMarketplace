using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<UpdateUserProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .Include(u => u.Photo)
            .FirstOrDefaultAsync(u => u.Id == (request.UserId), cancellationToken);

        if (user is null)
            throw new KeyNotFoundException($"User with id '{request.UserId}' was not found");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.Gender.HasValue) user.Gender = (int)request.Gender.Value;
        if (request.BirthDate.HasValue) user.BirthDate = request.BirthDate.Value;
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user profile: {errors}");
        }

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
            user.Photo is not null,
            user.Photo?.MimeType,
            user.Photo?.UploadedAt.Ticks
        );
    }
}