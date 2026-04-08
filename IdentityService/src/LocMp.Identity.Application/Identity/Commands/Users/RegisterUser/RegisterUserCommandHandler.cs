using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMapper mapper
) : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        const string defaultRoleName = nameof(UserRole.User);

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Gender = (int?)request.Gender,
            BirthDate = request.DateOfBirth,
            Active = true,
            RegisteredAt = DateTime.UtcNow,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to register user '{request.Email}': {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, defaultRoleName).ConfigureAwait(false);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(
                $"User was created but failed to assign role '{defaultRoleName}': {errors}");
        }

        return mapper.Map<UserDto>(user);
    }
}