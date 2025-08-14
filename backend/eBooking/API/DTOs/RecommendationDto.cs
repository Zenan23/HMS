using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RecommendationDto : BaseEntityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public int? UserId { get; set; }
        public int? HotelId { get; set; }
        public int? ServiceId { get; set; }
    }

    public class CreateRecommendationDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal? Price { get; set; }

        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5")]
        public int Priority { get; set; } = 3;

        public bool IsActive { get; set; } = true;

        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }

        [Url(ErrorMessage = "External URL must be a valid URL")]
        public string? ExternalUrl { get; set; }

        public int? UserId { get; set; }
        public int? HotelId { get; set; }
        public int? ServiceId { get; set; }
    }

    public class UpdateRecommendationDto : UpdateBaseEntityDto
    {
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal? Price { get; set; }

        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5")]
        public int Priority { get; set; } = 3;

        public bool IsActive { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }

        [Url(ErrorMessage = "External URL must be a valid URL")]
        public string? ExternalUrl { get; set; }

        public int? UserId { get; set; }
        public int? HotelId { get; set; }
        public int? ServiceId { get; set; }
    }

}
