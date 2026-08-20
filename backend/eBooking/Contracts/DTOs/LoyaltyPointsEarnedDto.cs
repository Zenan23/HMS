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
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }

        [Required(ErrorMessage = "Points earned is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points earned must be greater than 0")]
        public int PointsEarned { get; set; }

        [Required(ErrorMessage = "Earned date is required")]
        public DateTime EarnedAt { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateLoyaltyPointsEarnedDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }

        [Required(ErrorMessage = "Points earned is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points earned must be greater than 0")]
        public int PointsEarned { get; set; }

        [Required(ErrorMessage = "Earned date is required")]
        public DateTime EarnedAt { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
