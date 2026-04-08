using LocMp.Identity.Domain.Enums;

namespace LocMp.Identity.Api.Requests.Users;

public sealed record UpdateUserRequest(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    Gender? Gender,
    DateTime? DateOfBirth,
    bool Active
);