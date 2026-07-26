using EventHub.Domain.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace EventHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var fromEmail = _config["MailtrapSettings:FromEmail"] ?? "";
        var host = _config["MailtrapSettings:Host"] ?? "";
        var port = int.Parse(_config["MailtrapSettings:Port"] ?? "2525");
        var username = _config["MailtrapSettings:Username"] ?? "";
        var password = _config["MailtrapSettings:Password"] ?? "";

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(fromEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
        
        await smtp.AuthenticateAsync(username, password);
        
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}