using API.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class BookingStatusHistoryDto : BaseEntityDto
    {
        public BookingStatus FromStatus { get; set; }
        public BookingStatus ToStatus { get; set; }
        public DateTime ChangeDate { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public int BookingId { get; set; }
        public int? ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
    }

    public class CreateBookingStatusHistoryDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "From status is required")]
        public BookingStatus FromStatus { get; set; }

        [Required(ErrorMessage = "To status is required")]
        public BookingStatus ToStatus { get; set; }

        [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        public string? Reason { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        public int? ChangedByUserId { get; set; }
    }

    public class UpdateBookingStatusHistoryDto : UpdateBaseEntityDto
    {
        [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        public string? Reason { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
