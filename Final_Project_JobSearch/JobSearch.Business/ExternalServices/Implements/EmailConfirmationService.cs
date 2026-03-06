using JobSearch.Business.ExternalServices.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

namespace Job_Search_system.Business.ExternalServices.Implementations
{
    public class EmailConfirmationService : IEmailConfirmationService
    {
        private readonly IConfiguration _configuration;

        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public EmailConfirmationService(IConfiguration configuration)
        {
            _configuration = configuration;
            SmtpServer = _configuration["EmailConfirmationSettings:SmtpServer"];
            Port = int.Parse(_configuration["EmailConfirmationSettings:Port"] ?? "587");
            SenderName = _configuration["EmailConfirmationSettings:SenderName"];
            SenderEmail = _configuration["EmailConfirmationSettings:SenderEmail"];
            UserName = _configuration["EmailConfirmationSettings:UserName"];
            Password = _configuration["EmailConfirmationSettings:Password"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(SenderName, SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            // Təhlükəsizlik üçün TLS (Advanced Security)
            await smtp.ConnectAsync(SmtpServer, Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(UserName, Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}