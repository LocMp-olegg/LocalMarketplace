using LocMp.BuildingBlocks.Application.Exceptions;
﻿using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUserByPhoneNumber;

public sealed class GetUserByPhoneNumberQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    : IRequestHandler<GetUserByPhoneNumberQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByPhoneNumberQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ArgumentException("Phone number must be provided.");

        var user = await userManager.Users
                       .AsNoTracking()
                       .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken)
                       .ConfigureAwait(false)
                   ?? throw new NotFoundException($"User with phone '{request.PhoneNumber}' was not found.");

        var roles = await userManager.GetRolesAsync(user);
        return mapper.Map<UserDto>(user) with { Roles = [.. roles] };
    }
}