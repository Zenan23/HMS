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
        [Required(ErrorMessage = "Room ID is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Reported date is required")]
        public DateTime ReportedAt { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative")]
        public decimal Cost { get; set; }

        [StringLength(100, ErrorMessage = "Technician name cannot exceed 100 characters")]
        public string TechnicianName { get; set; } = string.Empty;
    }

    public class UpdateRoomMaintenanceLogDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Room ID is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Reported date is required")]
        public DateTime ReportedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative")]
        public decimal Cost { get; set; }

        [StringLength(100, ErrorMessage = "Technician name cannot exceed 100 characters")]
        public string TechnicianName { get; set; } = string.Empty;
    }
}
