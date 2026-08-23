using Application.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Persistence.Interfaces;

namespace Application.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        // Ne oslanjamo se na MailKit-ov default timeout (koji zna biti dug) — ako je SMTP server
        // nedostupan/blokiran (npr. mrežni problem unutar Docker kontejnera), radije brzo javimo
        // grešku u logu nego da zahtjev "visi" po minut-dva prije nego što se vidi šta se desilo.
        private const int TimeoutMs = 20000;

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
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = bodyHtml };

                // Port 465 = implicitni TLS od početka konekcije; svaki drugi port (npr. Gmail-ov
                // uobičajeni 587) = STARTTLS nakon plaintext handshake-a. Ovo su dvije različite
                // stvari — "Auto" bi trebao sam pogoditi, ali eksplicitno je pouzdanije za Gmail.
                var socketOptions = _options.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : _options.UseSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None;

                using var client = new SmtpClient
                {
                    Timeout = TimeoutMs
                };

                await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);

                if (!string.IsNullOrWhiteSpace(_options.Username))
                {
                    await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email '{Subject}' za {ToEmail} je uspješno poslan.", subject, toEmail);
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
