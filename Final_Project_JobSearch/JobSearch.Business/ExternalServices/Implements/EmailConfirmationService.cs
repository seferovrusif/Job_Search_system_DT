using JobSearch.Business.ExternalServices.Interfaces;
using JobSearch.Core.Entities;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
namespace JobSearch.Business.ExternalServices.Implements
{
    public class EmailConfirmationService : IEmailConfirmationService
    {
        private readonly EmailConfirmationSettings _settings;

        public EmailConfirmationService(IOptions<EmailConfirmationSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendConfirmationEmailAsync(AppUser user, string confirmationLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _settings.SenderName,
                _settings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(user.Email));
            message.Subject = "Confirm your email";

            message.Body = new BodyBuilder
            {
                HtmlBody = $@"
                <h3>Hello {user.UserName}</h3>
                <p>Please confirm your email by clicking the link below:</p>
                <a href='{confirmationLink}'>Confirm Email</a>"
            }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _settings.SmtpServer,
                _settings.Port,
                MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _settings.UserName,
                _settings.Password);

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
