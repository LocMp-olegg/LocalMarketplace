using LocMp.Identity.Domain.Enums;

namespace LocMp.Identity.Application.DTOs.UserProfile;

public sealed record UserProfileDto(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    Gender? Gender,
    DateOnly? BirthDate,
    string? PhoneNumber,
    DateTimeOffset RegisteredAt,
    bool HasPhoto,
    string? PhotoMimeType,
    long? PhotoVersion,
    IReadOnlyList<string> Roles
);