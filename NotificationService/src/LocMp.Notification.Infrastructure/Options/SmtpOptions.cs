namespace LocMp.Notification.Infrastructure.Options;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; } = false;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "noreply@localmarketplace.local";
    public string FromName { get; set; } = "Районный Маркетплейс";
}