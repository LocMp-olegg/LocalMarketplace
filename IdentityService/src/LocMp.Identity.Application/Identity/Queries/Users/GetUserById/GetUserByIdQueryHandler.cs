using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUserById;

public sealed class GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString()).ConfigureAwait(false);

        if (user is null)
            throw new KeyNotFoundException($"User with id '{request.Id}' was not found.");

        return mapper.Map<UserDto>(user);
    }
}

