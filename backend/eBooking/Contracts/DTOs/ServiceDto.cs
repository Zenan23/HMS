using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class ServiceDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
    }

    public class CreateServiceDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Hotel ID is required")]
        public int HotelId { get; set; }
    }

    public class UpdateServiceDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Hotel ID is required")]
        public int HotelId { get; set; }
    }

}
