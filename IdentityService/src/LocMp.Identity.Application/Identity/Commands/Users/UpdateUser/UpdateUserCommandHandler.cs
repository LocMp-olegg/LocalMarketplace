using LocMp.BuildingBlocks.Application.Exceptions;
using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            throw new NotFoundException($"User with id '{request.Id}' was not found.");

        if (!string.IsNullOrEmpty(request.PhoneNumber) &&
            await userManager.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != request.Id, cancellationToken))
            throw new ConflictException($"Phone number '{request.PhoneNumber}' is already in use.");

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.Gender = (int?)request.Gender;
        user.BirthDate = request.BirthDate;
        user.Active = request.Active;

        var result = await userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Code switch
            {
                "DuplicateEmail" => "Email is already in use.",
                "DuplicateUserName" => "Username is already taken.",
                _ => e.Description
            }));
            throw new ConflictException($"Update failed: {errors}");
        }

        var roles = await userManager.GetRolesAsync(user);
        return mapper.Map<UserDto>(user) with { Roles = [.. roles] };
    }
}