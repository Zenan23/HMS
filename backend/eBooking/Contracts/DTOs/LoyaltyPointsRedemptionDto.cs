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
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Points used is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points used must be greater than 0")]
        public int PointsUsed { get; set; }

        [Required(ErrorMessage = "Redeemed date is required")]
        public DateTime RedeemedAt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Equivalent value amount must be non-negative")]
        public decimal EquivalentValueAmount { get; set; }
    }

    public class UpdateLoyaltyPointsRedemptionDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Points used is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points used must be greater than 0")]
        public int PointsUsed { get; set; }

        [Required(ErrorMessage = "Redeemed date is required")]
        public DateTime RedeemedAt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Equivalent value amount must be non-negative")]
        public decimal EquivalentValueAmount { get; set; }
    }
}
