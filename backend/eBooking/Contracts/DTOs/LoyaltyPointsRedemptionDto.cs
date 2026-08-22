using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class LoyaltyPointsRedemptionDto : BaseEntityDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public string BookingLabel { get; set; } = string.Empty;
        public int PointsUsed { get; set; }
        public DateTime RedeemedAt { get; set; }
        public decimal EquivalentValueAmount { get; set; }
    }

    public class CreateLoyaltyPointsRedemptionDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ID rezervacije je obavezan.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Broj iskorištenih bodova je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Broj iskorištenih bodova mora biti veći od 0.")]
        public int PointsUsed { get; set; }

        [Required(ErrorMessage = "Datum iskorištavanja je obavezan.")]
        public DateTime RedeemedAt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Ekvivalentna vrijednost ne smije biti negativna.")]
        public decimal EquivalentValueAmount { get; set; }
    }

    public class UpdateLoyaltyPointsRedemptionDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ID rezervacije je obavezan.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Broj iskorištenih bodova je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Broj iskorištenih bodova mora biti veći od 0.")]
        public int PointsUsed { get; set; }

        [Required(ErrorMessage = "Datum iskorištavanja je obavezan.")]
        public DateTime RedeemedAt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Ekvivalentna vrijednost ne smije biti negativna.")]
        public decimal EquivalentValueAmount { get; set; }
    }
}
