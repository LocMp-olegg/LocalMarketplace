namespace LocMp.Identity.Application.DTOs.User;

public sealed record UserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public int? Gender { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string PhoneNumber { get; init; } = null!;
    public DateTimeOffset RegisteredAt { get; init; }
    public bool Active { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}