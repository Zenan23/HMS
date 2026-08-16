using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class InventoryItemDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int MinimumStockLevel { get; set; }
    }

    public class CreateInventoryItemDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv artikla je obavezan.")]
        [StringLength(150, ErrorMessage = "Naziv ne smije biti duži od 150 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jedinica mjere je obavezna (npr. kom, kg, l).")]
        [StringLength(20, ErrorMessage = "Jedinica mjere ne smije biti duža od 20 karaktera.")]
        public string Unit { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Kategorija ne smije biti duža od 100 karaktera.")]
        public string Category { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Minimalna zaliha mora biti 0 ili veća.")]
        public int MinimumStockLevel { get; set; }
    }

    public class UpdateInventoryItemDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Naziv artikla je obavezan.")]
        [StringLength(150, ErrorMessage = "Naziv ne smije biti duži od 150 karaktera.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jedinica mjere je obavezna (npr. kom, kg, l).")]
        [StringLength(20, ErrorMessage = "Jedinica mjere ne smije biti duža od 20 karaktera.")]
        public string Unit { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Kategorija ne smije biti duža od 100 karaktera.")]
        public string Category { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Minimalna zaliha mora biti 0 ili veća.")]
        public int MinimumStockLevel { get; set; }
    }
}
