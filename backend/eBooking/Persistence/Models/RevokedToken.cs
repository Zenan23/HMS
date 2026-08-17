namespace Persistence.Models
{
    /// <summary>
    /// Evidencija JWT tokena koji su eksplicitno poništeni prije prirodnog isteka (logout). JWT je
    /// po prirodi stateless i ne može se "obrisati" na klijentu — server mora čuvati listu poništenih
    /// tokena (po jti) i odbijati ih pri validaciji, dok im ne istekne originalni rok trajanja.
    /// Uputa: "Logout mora invalidirati token na serveru, a ne samo lokalno obrisati token."
    /// </summary>
    public class RevokedToken
    {
        public int Id { get; set; }

        /// <summary>JWT ID (jti claim) tokena koji je poništen.</summary>
        public string Jti { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User? User { get; set; }

        /// <summary>Originalni datum isteka tokena — nakon ovog trenutka zapis se može očistiti (purge).</summary>
        public DateTime ExpiresAt { get; set; }

        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    }
}
