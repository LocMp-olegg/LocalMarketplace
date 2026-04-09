using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Enums;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    Gender? Gender,
    DateOnly? BirthDate,
    bool IsSeller = false,
    double? Latitude = null,
    double? Longitude = null
) : IRequest<UserDto>;
