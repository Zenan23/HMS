using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class CityDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
    }

    public class CreateCityDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv grada je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv grada ne smije biti duži od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Država je obavezna.")]
        public int CountryId { get; set; }
    }

    public class UpdateCityDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv grada je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv grada ne smije biti duži od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Država je obavezna.")]
        public int CountryId { get; set; }
    }
}
