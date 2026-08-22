using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class LoyaltyPointsEarnedDto : BaseEntityDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public string? BookingLabel { get; set; }
        public int? PaymentId { get; set; }
        public int PointsEarned { get; set; }
        public DateTime EarnedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // Create/Update postoje za ručne korekcije od strane osoblja (npr. bonus bodovi, ispravka
    // greške) — automatsko zarađivanje ide direktno kroz DbContext u PaymentService, ne preko ovog DTO-a.
    public class CreateLoyaltyPointsEarnedDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }

        [Required(ErrorMessage = "Broj osvojenih bodova je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Broj osvojenih bodova mora biti veći od 0.")]
        public int PointsEarned { get; set; }

        [Required(ErrorMessage = "Datum osvajanja bodova je obavezan.")]
        public DateTime EarnedAt { get; set; }

        [Required(ErrorMessage = "Razlog je obavezan.")]
        [StringLength(200, ErrorMessage = "Razlog ne smije imati više od 200 karaktera.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateLoyaltyPointsEarnedDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }

        [Required(ErrorMessage = "Broj osvojenih bodova je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Broj osvojenih bodova mora biti veći od 0.")]
        public int PointsEarned { get; set; }

        [Required(ErrorMessage = "Datum osvajanja bodova je obavezan.")]
        public DateTime EarnedAt { get; set; }

        [Required(ErrorMessage = "Razlog je obavezan.")]
        [StringLength(200, ErrorMessage = "Razlog ne smije imati više od 200 karaktera.")]
        public string Reason { get; set; } = string.Empty;
    }
}
