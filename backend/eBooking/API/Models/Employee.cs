namespace API.Models
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty; // e.g., "Manager", "Receptionist", "Housekeeper", "Maintenance"
        public string Department { get; set; } = string.Empty; // e.g., "Front Desk", "Housekeeping", "Maintenance", "Management"
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;
        public string Address { get; set; } = string.Empty;
        public DateTime? TerminationDate { get; set; }
        public int HotelId { get; set; }
        public int? UserId { get; set; }
        public Hotel Hotel { get; set; } = null!;
        public User? User { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
