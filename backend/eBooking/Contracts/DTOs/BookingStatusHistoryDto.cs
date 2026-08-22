using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
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
        [Required(ErrorMessage = "Prethodni status je obavezan.")]
        public BookingStatus FromStatus { get; set; }

        [Required(ErrorMessage = "Novi status je obavezan.")]
        public BookingStatus ToStatus { get; set; }

        [StringLength(200, ErrorMessage = "Razlog ne smije imati više od 200 karaktera.")]
        public string? Reason { get; set; }

        [StringLength(500, ErrorMessage = "Napomene ne smiju imati više od 500 karaktera.")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "ID rezervacije je obavezan.")]
        public int BookingId { get; set; }

        public int? ChangedByUserId { get; set; }
    }

    public class UpdateBookingStatusHistoryDto : UpdateBaseEntityDto
    {
        [StringLength(200, ErrorMessage = "Razlog ne smije imati više od 200 karaktera.")]
        public string? Reason { get; set; }

        [StringLength(500, ErrorMessage = "Napomene ne smiju imati više od 500 karaktera.")]
        public string? Notes { get; set; }
    }
}
