using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Enums;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    Gender? Gender,
    DateTime? DateOfBirth,
    bool Active = true
) : IRequest<UserDto>;

