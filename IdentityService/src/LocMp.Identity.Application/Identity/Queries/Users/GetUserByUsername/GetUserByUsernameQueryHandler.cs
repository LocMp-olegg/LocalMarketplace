using AutoMapper;
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

        var user = await userManager.FindByNameAsync(request.Username).ConfigureAwait(false);

        if (user is null)
            throw new KeyNotFoundException($"User with username '{request.Username}' was not found.");

        return mapper.Map<UserDto>(user);
    }
}