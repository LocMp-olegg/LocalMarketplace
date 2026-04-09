using LocMp.Identity.Domain.Enums;

namespace LocMp.Identity.Application.DTOs.UserProfile;

public sealed record UserProfileDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public Gender? Gender { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
    public bool HasPhoto { get; init; }
    public string? PhotoMimeType { get; init; }
    public long? PhotoVersion { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}