using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class HotelDto : BaseEntityDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StarRating { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CreateHotelDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Hotel name is required")]
        [StringLength(100, ErrorMessage = "Hotel name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
        public string Country { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Star rating must be between 0 and 5")]
        public int StarRating { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }

    public class UpdateHotelDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Hotel name is required")]
        [StringLength(100, ErrorMessage = "Hotel name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
        public string Country { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Star rating must be between 0 and 5")]
        public int StarRating { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }

}
