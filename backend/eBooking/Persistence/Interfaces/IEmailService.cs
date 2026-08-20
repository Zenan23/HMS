namespace Persistence.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Šalje email preko SMTP-a. Ako SMTP nije konfigurisan (prazan Host u .env), poziv se
        /// tiho preskače uz upozorenje u logu — ne smije srušiti tok koji ga poziva (npr. forgot
        /// password i dalje mora vratiti generički uspješan odgovor, bez otkrivanja da li je
        /// email zaista poslan, radi sprječavanja enumeracije korisnika).
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
    }
}
