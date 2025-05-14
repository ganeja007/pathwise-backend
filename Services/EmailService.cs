using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PathwiseAPI.Models;

namespace PathwiseAPI.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendPasswordResetEmail(string toEmail, string resetLink)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = "PathWise - Password Reset Instructions",
                Body = $"Hello,\n\nTo reset your password, please click the link below:\n\n{resetLink}\n\nIf you didn’t request this, you can ignore this email.\n\nBest,\nPathWise Team",
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
