namespace API.Models
{
    public class Recommendation : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; 
        public decimal? Price { get; set; }
        public int Priority { get; set; } = 1; 
        public bool IsActive { get; set; } = true;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExternalUrl { get; set; } 
        public int? UserId { get; set; } 
        public int? HotelId { get; set; } 
        public int? ServiceId { get; set; } 
        public User? User { get; set; }
        public Hotel? Hotel { get; set; }
        public Service? Service { get; set; }
    }

}
