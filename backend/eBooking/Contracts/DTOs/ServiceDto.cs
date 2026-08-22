using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class ServiceDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ServiceCategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
    }

    public class CreateServiceDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv usluge je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv usluge ne smije imati više od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cijena je obavezna.")]
        [Range(0, double.MaxValue, ErrorMessage = "Cijena ne smije biti negativna.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kategorija je obavezna.")]
        public int ServiceCategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "ID hotela je obavezan.")]
        public int HotelId { get; set; }
    }

    public class UpdateServiceDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv usluge je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv usluge ne smije imati više od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cijena je obavezna.")]
        [Range(0, double.MaxValue, ErrorMessage = "Cijena ne smije biti negativna.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kategorija je obavezna.")]
        public int ServiceCategoryId { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "ID hotela je obavezan.")]
        public int HotelId { get; set; }
    }

}
