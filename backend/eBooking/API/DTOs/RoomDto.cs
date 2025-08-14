using API.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RoomDto : BaseEntityDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxOccupancy { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
    }

    public class CreateRoomDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Room number is required")]
        [StringLength(10, ErrorMessage = "Room number cannot exceed 10 characters")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room type is required")]
        public RoomType RoomType { get; set; }

        [Required(ErrorMessage = "Price per night is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price per night must be greater than 0")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Max occupancy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Max occupancy must be at least 1")]
        public int MaxOccupancy { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Hotel ID is required")]
        public int HotelId { get; set; }
    }

    public class UpdateRoomDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Room number is required")]
        [StringLength(10, ErrorMessage = "Room number cannot exceed 10 characters")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room type is required")]
        public RoomType RoomType { get; set; }

        [Required(ErrorMessage = "Price per night is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price per night must be greater than 0")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Max occupancy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Max occupancy must be at least 1")]
        public int MaxOccupancy { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "Hotel ID is required")]
        public int HotelId { get; set; }
    }

}
