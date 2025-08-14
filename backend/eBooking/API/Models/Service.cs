namespace API.Models
{
    public class Service : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty; // e.g., "Spa", "Restaurant", "Room Service", "Transportation"
        public bool IsAvailable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;
    }
}
