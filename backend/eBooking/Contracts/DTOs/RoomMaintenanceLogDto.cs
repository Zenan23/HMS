using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class RoomMaintenanceLogDto : BaseEntityDto
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
    }

    public class CreateRoomMaintenanceLogDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "ID sobe je obavezan.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Datum prijave kvara je obavezan.")]
        public DateTime ReportedAt { get; set; }

        [StringLength(1000, ErrorMessage = "Opis ne smije imati više od 1000 karaktera.")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Trošak ne smije biti negativan.")]
        public decimal Cost { get; set; }

        [StringLength(100, ErrorMessage = "Ime tehničara ne smije imati više od 100 karaktera.")]
        public string TechnicianName { get; set; } = string.Empty;
    }

    public class UpdateRoomMaintenanceLogDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "ID sobe je obavezan.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Datum prijave kvara je obavezan.")]
        public DateTime ReportedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [StringLength(1000, ErrorMessage = "Opis ne smije imati više od 1000 karaktera.")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Trošak ne smije biti negativan.")]
        public decimal Cost { get; set; }

        [StringLength(100, ErrorMessage = "Ime tehničara ne smije imati više od 100 karaktera.")]
        public string TechnicianName { get; set; } = string.Empty;
    }
}
