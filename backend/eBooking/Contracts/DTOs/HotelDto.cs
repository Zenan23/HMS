using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class HotelDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StarRating { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Popunjava se SAMO u odgovoru recommender endpointa — objašnjava korisniku zbog čega je
        /// baš ovaj hotel preporučen (npr. "slični korisnici ga visoko ocjenjuju"). Uputa: "Recommender
        /// mora korisniku objašnjavati zbog čega se određeni sadržaj preporučuje - objašnjive preporuke."
        /// </summary>
        public string? RecommendationReason { get; set; }
    }

    public class CreateHotelDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv hotela je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv hotela ne smije imati više od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adresa je obavezna.")]
        [StringLength(200, ErrorMessage = "Adresa ne smije imati više od 200 karaktera.")]
        public string Address { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Grad je obavezan.")]
        public int CityId { get; set; }

        [StringLength(20, ErrorMessage = "Broj telefona ne smije imati više od 20 karaktera.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        [StringLength(100, ErrorMessage = "Email ne smije imati više od 100 karaktera.")]
        public string Email { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Ocjena hotela mora biti između 0 i 5.")]
        public int StarRating { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }

    public class UpdateHotelDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv hotela je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv hotela ne smije imati više od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adresa je obavezna.")]
        [StringLength(200, ErrorMessage = "Adresa ne smije imati više od 200 karaktera.")]
        public string Address { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Grad je obavezan.")]
        public int CityId { get; set; }

        [StringLength(20, ErrorMessage = "Broj telefona ne smije imati više od 20 karaktera.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        [StringLength(100, ErrorMessage = "Email ne smije imati više od 100 karaktera.")]
        public string Email { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Ocjena hotela mora biti između 0 i 5.")]
        public int StarRating { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }

}
