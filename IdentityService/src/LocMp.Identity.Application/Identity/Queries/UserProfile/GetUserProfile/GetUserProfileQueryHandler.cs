using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.UserProfile.GetUserProfile;

public sealed class GetUserProfileQueryHandler(
    UserManager<ApplicationUser> userManager,
    IMapper mapper
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
        return mapper.Map<UserProfileDto>(user) with { Roles = [.. roles] };
    }
}