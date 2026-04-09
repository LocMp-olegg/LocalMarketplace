using LocMp.Identity.Application.DTOs.UserProfile;
using LocMp.Identity.Domain.Enums;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    Gender? Gender,
    DateOnly? BirthDate,
    string? PhoneNumber,
    bool? IsSeller = null
) : IRequest<UserProfileDto>;