using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class PriceAdjustmentDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PercentageModifier { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCumulative { get; set; }
    }

    public class CreatePriceAdjustmentDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(-100, 1000, ErrorMessage = "Percentage modifier must be between -100 and 1000")]
        public decimal PercentageModifier { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public bool IsCumulative { get; set; }
    }

    public class UpdatePriceAdjustmentDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(-100, 1000, ErrorMessage = "Percentage modifier must be between -100 and 1000")]
        public decimal PercentageModifier { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public bool IsCumulative { get; set; }
    }
}
