using HotelDemo.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HotelDemo.Services;

public class EmailService(IOptions<SmtpSettings> smtpSettings) : IEmailService
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

    public Func<ISmtpClient> SmtpClientFactory { get; set; } = () => new SmtpClient();

    public async Task SendVerificationEmailAsync(string toEmail, string verificationCode)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Email Verification";
        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is {verificationCode}"
        };

        using var client = SmtpClientFactory();
        await client.ConnectAsync(
            _smtpSettings.Server,
            _smtpSettings.Port,
            SecureSocketOptions.StartTlsWhenAvailable
        );
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
