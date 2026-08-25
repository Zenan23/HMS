using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
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

        // Audit trag moderacije — server-set, nikad se ne postavlja kroz Create/UpdateReviewDto.
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }
    }

    public class CreateReviewDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Ocjena je obavezna.")]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti između 1 i 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Komentar je obavezan.")]
        [StringLength(1000, ErrorMessage = "Komentar ne smije imati više od 1000 karaktera.")]
        public string Comment { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID hotela je obavezan.")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "ID korisnika je obavezan.")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    public class UpdateReviewDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Ocjena je obavezna.")]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti između 1 i 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Komentar je obavezan.")]
        [StringLength(1000, ErrorMessage = "Komentar ne smije imati više od 1000 karaktera.")]
        public string Comment { get; set; } = string.Empty;

        public bool IsApproved { get; set; }
    }

}
