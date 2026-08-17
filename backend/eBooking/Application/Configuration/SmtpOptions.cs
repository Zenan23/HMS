namespace Application.Configuration
{
    /// <summary>
    /// SMTP konfiguracija za slanje emailova (trenutno: reset lozinke). Uputa: "Svi konfiguracijski
    /// podaci moraju biti smješteni u konfiguracijske datoteke (.env datoteka)... SMTP podatke
    /// (host, username, password, use ssl i port)." — čita se iz env.docker/.env, ne appsettings.json.
    /// </summary>
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "eBooking";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(FromEmail);
    }
}
