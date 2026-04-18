using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMapper mapper,
    IEventBus eventBus
) : IRequestHandler<UpdateUserProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var user = await userManager.Users
                       .Include(u => u.Photo)
                       .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                   ?? throw new NotFoundException($"User with id '{request.UserId}' was not found");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.Gender.HasValue) user.Gender = (int)request.Gender.Value;
        if (request.BirthDate.HasValue) user.BirthDate = request.BirthDate.Value;
        if (request.PhoneNumber is not null)
        {
            var phoneTaken = await userManager.Users
                .AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != request.UserId, ct);
            if (phoneTaken)
                throw new ConflictException("Phone number is already in use.");
            user.PhoneNumber = request.PhoneNumber;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user profile: {errors}");
        }

        if (request.IsSeller == true)
        {
            var isAlreadySeller = await userManager.IsInRoleAsync(user, nameof(UserRole.Seller));
            if (!isAlreadySeller)
            {
                await userManager.AddToRoleAsync(user, nameof(UserRole.Seller));
                var displayName = $"{user.FirstName} {user.LastName}".Trim();
                await eventBus.PublishAsync(
                    new UserBecameSellerEvent(user.Id, displayName, DateTimeOffset.UtcNow), ct);
            }
        }

        await eventBus.PublishAsync(
            new UserProfileUpdatedEvent(
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim(),
                AvatarUrl: null,
                DateTimeOffset.UtcNow), ct);

        var roles = await userManager.GetRolesAsync(user);
        return mapper.Map<UserProfileDto>(user) with { Roles = [.. roles] };
    }
}