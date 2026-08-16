using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class CountryDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateCountryDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv države je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv države ne smije biti duži od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateCountryDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv države je obavezan.")]
        [StringLength(100, ErrorMessage = "Naziv države ne smije biti duži od 100 karaktera.")]
        public string Name { get; set; } = string.Empty;
    }
}
