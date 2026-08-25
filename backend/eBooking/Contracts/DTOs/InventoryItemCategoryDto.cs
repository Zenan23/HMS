using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class InventoryItemCategoryDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateInventoryItemCategoryDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv kategorije je obavezan.")]
        [StringLength(50, ErrorMessage = "Naziv kategorije ne smije biti duži od 50 karaktera.")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateInventoryItemCategoryDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv kategorije je obavezan.")]
        [StringLength(50, ErrorMessage = "Naziv kategorije ne smije biti duži od 50 karaktera.")]
        public string Name { get; set; } = string.Empty;
    }
}
