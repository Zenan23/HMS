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
        public int? CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public int? HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
    }

    public class CreatePriceAdjustmentDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv ne smije imati više od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Range(-100, 1000, ErrorMessage = "Procentualni modifikator mora biti između -100 i 1000.")]
        public decimal PercentageModifier { get; set; }

        [Required(ErrorMessage = "Datum početka je obavezan.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Datum završetka je obavezan.")]
        public DateTime EndDate { get; set; }

        public bool IsCumulative { get; set; }

        /// <summary>Server-side se postavlja iz JWT-a (vidi PriceAdjustmentsController) — klijent ovo ne treba slati.</summary>
        public int? CreatedByUserId { get; set; }

        /// <summary>Null = pravilo važi za sve hotele. Popunjeno = važi samo za taj hotel.</summary>
        public int? HotelId { get; set; }
    }

    public class UpdatePriceAdjustmentDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv ne smije imati više od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Range(-100, 1000, ErrorMessage = "Procentualni modifikator mora biti između -100 i 1000.")]
        public decimal PercentageModifier { get; set; }

        [Required(ErrorMessage = "Datum početka je obavezan.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Datum završetka je obavezan.")]
        public DateTime EndDate { get; set; }

        public bool IsCumulative { get; set; }

        /// <summary>Null = pravilo važi za sve hotele. Popunjeno = važi samo za taj hotel.</summary>
        public int? HotelId { get; set; }
    }
}
