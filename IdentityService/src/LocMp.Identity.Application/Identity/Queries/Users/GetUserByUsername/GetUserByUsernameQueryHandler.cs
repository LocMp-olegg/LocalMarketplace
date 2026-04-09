using LocMp.BuildingBlocks.Application.Exceptions;
﻿using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUserByUsername;

public sealed class GetUserByUsernameQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    : IRequestHandler<GetUserByUsernameQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username must be provided.");

        var user = await userManager.FindByNameAsync(request.Username).ConfigureAwait(false)
                   ?? throw new NotFoundException($"User with username '{request.Username}' was not found.");

        var roles = await userManager.GetRolesAsync(user);
        return mapper.Map<UserDto>(user) with { Roles = [.. roles] };
    }
}