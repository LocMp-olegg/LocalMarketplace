using LocMp.Notification.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace LocMp.Notification.Infrastructure.Services;

public sealed class SmtpEmailService(
    IOptions<SmtpOptions> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var opts = options.Value;

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(opts.FromName, opts.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var client = new SmtpClient();
            var socketOptions = opts.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None;
            await client.ConnectAsync(opts.Host, opts.Port, socketOptions, ct);

            if (!string.IsNullOrEmpty(opts.UserName))
                if (opts.Password != null)
                    await client.AuthenticateAsync(opts.UserName, opts.Password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
        }
    }
}