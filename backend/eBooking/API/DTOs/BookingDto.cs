using API.Enums;
using System.Text.Json.Serialization;

namespace API.DTOs
{
    public class BookingDto : BaseEntityDto
    {
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string SpecialRequests { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public IEnumerable<BookingServiceItemDto>? Services { get; set; }
    }

    public class CreateBookingDto : CreateBaseEntityDto
    {
        [JsonPropertyName("checkIn")]
        public DateTime CheckInDate { get; set; }
        [JsonPropertyName("checkOut")]
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public string SpecialRequests { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public IEnumerable<BookingServiceCreateItemDto>? Services { get; set; }
    }

    public class UpdateBookingDto : UpdateBaseEntityDto
    {
        [JsonPropertyName("checkIn")]
        public DateTime CheckInDate { get; set; }
        [JsonPropertyName("checkOut")]
        public DateTime CheckOutDate { get; set; }
        public BookingStatus Status { get; set; }
        public int NumberOfGuests { get; set; }
        public string SpecialRequests { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }

    public class BookingServiceCreateItemDto
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class BookingServiceItemDto
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ServiceName { get; set; }
    }

}
