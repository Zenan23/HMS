namespace Persistence.Models
{
    public class RoomMaintenanceLog : BaseEntity
    {
        public int RoomId { get; set; }
        public DateTime ReportedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public Room Room { get; set; } = null!;
    }
}
