using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReviewDto : BaseEntityDto
    {
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? BookingId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateReviewDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hotel ID is required")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    public class UpdateReviewDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; } = string.Empty;

        public bool IsApproved { get; set; }
    }

}
