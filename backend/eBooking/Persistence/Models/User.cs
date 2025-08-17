using Contracts.Enums;

namespace Persistence.Models
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public UserRole Role { get; set; } = UserRole.Guest;
        public int? HotelId { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

}
