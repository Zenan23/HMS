using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
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
        [Required(ErrorMessage = "Broj sobe je obavezan.")]
        [StringLength(10, ErrorMessage = "Broj sobe ne smije imati više od 10 karaktera.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tip sobe je obavezan.")]
        public RoomType RoomType { get; set; }

        [Required(ErrorMessage = "Cijena po noćenju je obavezna.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cijena po noćenju mora biti veća od 0.")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Maksimalan broj gostiju je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Maksimalan broj gostiju mora biti najmanje 1.")]
        public int MaxOccupancy { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "ID hotela je obavezan.")]
        public int HotelId { get; set; }
    }

    public class UpdateRoomDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Broj sobe je obavezan.")]
        [StringLength(10, ErrorMessage = "Broj sobe ne smije imati više od 10 karaktera.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tip sobe je obavezan.")]
        public RoomType RoomType { get; set; }

        [Required(ErrorMessage = "Cijena po noćenju je obavezna.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cijena po noćenju mora biti veća od 0.")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Maksimalan broj gostiju je obavezan.")]
        [Range(1, int.MaxValue, ErrorMessage = "Maksimalan broj gostiju mora biti najmanje 1.")]
        public int MaxOccupancy { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "ID hotela je obavezan.")]
        public int HotelId { get; set; }
    }

}
