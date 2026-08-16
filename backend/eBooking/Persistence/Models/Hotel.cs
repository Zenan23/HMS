namespace Persistence.Models
{
    public class Hotel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int CityId { get; set; }
        public City? City { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StarRating { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
