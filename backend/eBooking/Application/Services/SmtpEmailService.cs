using System.Net;
using System.Net.Mail;
using Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Interfaces;

namespace Application.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
        {
            if (!_options.IsConfigured)
            {
                _logger.LogWarning(
                    "SMTP nije konfigurisan (Smtp:Host prazan u .env) — email '{Subject}' za {ToEmail} NIJE poslan.",
                    subject, toEmail);
                return;
            }

            try
            {
                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.UseSsl,
                    Credentials = string.IsNullOrWhiteSpace(_options.Username)
                        ? null
                        : new NetworkCredential(_options.Username, _options.Password)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_options.FromEmail, _options.FromName),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                // Greška pri slanju emaila ne smije srušiti poslovni tok (npr. forgot-password) —
                // loguje se sa dovoljno informacija za reprodukciju, korisniku se i dalje vraća
                // generička poruka.
                _logger.LogError(ex, "Slanje emaila '{Subject}' za {ToEmail} nije uspjelo.", subject, toEmail);
            }
        }
    }
}
