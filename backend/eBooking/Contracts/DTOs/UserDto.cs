using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class UserDto : BaseEntityDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } 
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }

    public class CreateUserDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [StringLength(50, ErrorMessage = "Korisničko ime ne smije imati više od 50 karaktera.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        [StringLength(100, ErrorMessage = "Email ne smije imati više od 100 karaktera.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,100}$",
            ErrorMessage = "Lozinka mora imati najmanje 8 karaktera, veliko i malo slovo, broj i specijalni karakter.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, ErrorMessage = "Ime ne smije imati više od 50 karaktera.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, ErrorMessage = "Prezime ne smije imati više od 50 karaktera.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Broj telefona je obavezan.")]
        [StringLength(20, ErrorMessage = "Broj telefona ne smije imati više od 20 karaktera.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [StringLength(50, ErrorMessage = "Korisničko ime ne smije imati više od 50 karaktera.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        [StringLength(100, ErrorMessage = "Email ne smije imati više od 100 karaktera.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, ErrorMessage = "Ime ne smije imati više od 50 karaktera.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, ErrorMessage = "Prezime ne smije imati više od 50 karaktera.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Broj telefona je obavezan.")]
        [StringLength(20, ErrorMessage = "Broj telefona ne smije imati više od 20 karaktera.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; }
    }
}
