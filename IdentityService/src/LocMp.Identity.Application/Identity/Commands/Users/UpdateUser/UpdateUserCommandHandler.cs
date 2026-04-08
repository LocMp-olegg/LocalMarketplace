using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Users.UpdateUser;

public sealed class UpdateUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMapper mapper
) : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString()).ConfigureAwait(false);

        if (user is null)
            throw new KeyNotFoundException($"User with id '{request.Id}' was not found.");

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.Gender = (int?)request.Gender;
        user.BirthDate = request.DateOfBirth;
        user.Active = request.Active;

        var result = await userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user '{request.Email}': {errors}");
        }

        return mapper.Map<UserDto>(user);
    }
}