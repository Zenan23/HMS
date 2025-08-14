using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class EmployeeDto : BaseEntityDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime? TerminationDate { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class CreateEmployeeDto : CreateBaseEntityDto
    {
        [Required][StringLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required][StringLength(50)] public string LastName { get; set; } = string.Empty;
        [Required][EmailAddress][StringLength(100)] public string Email { get; set; } = string.Empty;
        [StringLength(20)] public string PhoneNumber { get; set; } = string.Empty;
        [Required][StringLength(100)] public string Position { get; set; } = string.Empty;
        [Required][StringLength(100)] public string Department { get; set; } = string.Empty;
        [Required] public DateTime HireDate { get; set; }
        [Required][Range(0, double.MaxValue)] public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;
        [StringLength(200)] public string Address { get; set; } = string.Empty;
        [Required] public int HotelId { get; set; }
        public int? UserId { get; set; }
    }

    public class UpdateEmployeeDto : UpdateBaseEntityDto
    {
        [Required][StringLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required][StringLength(50)] public string LastName { get; set; } = string.Empty;
        [Required][EmailAddress][StringLength(100)] public string Email { get; set; } = string.Empty;
        [StringLength(20)] public string PhoneNumber { get; set; } = string.Empty;
        [Required][StringLength(100)] public string Position { get; set; } = string.Empty;
        [Required][StringLength(100)] public string Department { get; set; } = string.Empty;
        [Required] public DateTime HireDate { get; set; }
        [Required][Range(0, double.MaxValue)] public decimal Salary { get; set; }
        public bool IsActive { get; set; }
        [StringLength(200)] public string Address { get; set; } = string.Empty;
        public DateTime? TerminationDate { get; set; }
        [Required] public int HotelId { get; set; }
        public int? UserId { get; set; }
    }

}
