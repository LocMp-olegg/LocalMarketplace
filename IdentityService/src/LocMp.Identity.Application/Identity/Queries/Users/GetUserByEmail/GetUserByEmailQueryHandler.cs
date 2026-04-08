using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUserByEmail;

public sealed class GetUserByEmailQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    : IRequestHandler<GetUserByEmailQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email must be provided.", nameof(request.Email));

        var email = request.Email.Trim();

        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (user is null)
            throw new KeyNotFoundException($"User with email '{request.Email}' was not found.");

        return mapper.Map<UserDto>(user);
    }
}