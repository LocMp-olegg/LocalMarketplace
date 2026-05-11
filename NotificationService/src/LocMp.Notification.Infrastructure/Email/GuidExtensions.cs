namespace LocMp.Notification.Infrastructure.Email;

internal static class GuidExtensions
{
    public static string ToShortId(this Guid id) =>
        id.ToString("N")[^8..].ToUpperInvariant();
}