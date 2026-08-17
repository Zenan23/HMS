namespace Persistence.Models
{
    /// <summary>
    /// Kod za reset lozinke (mobilna aplikacija — "zaboravljena lozinka"). Kod se šalje korisniku
    /// emailom, a ovdje se čuva isključivo HASHOVAN (isti IPasswordService kao za lozinke), sa
    /// definisanim istekom — nikad u plain text formatu (Dodatak A.3 uputa za seminarski rad).
    /// </summary>
    public class PasswordResetToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
