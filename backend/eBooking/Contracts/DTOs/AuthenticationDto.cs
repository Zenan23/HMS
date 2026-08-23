
using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [MinLength(6, ErrorMessage = "Lozinka mora imati najmanje 6 karaktera.")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [MinLength(3, ErrorMessage = "Korisničko ime mora imati najmanje 3 karaktera.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,100}$",
            ErrorMessage = "Lozinka mora imati najmanje 8 karaktera, veliko i malo slovo, broj i specijalni karakter.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [Compare("Password", ErrorMessage = "Lozinke se ne poklapaju.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime je obavezno.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Broj telefona je obavezan.")]
        [Phone(ErrorMessage = "Nevažeći format broja telefona.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Nevažeći format email-a.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod mora imati tačno 6 cifara.")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,100}$",
            ErrorMessage = "Nova lozinka mora imati najmanje 8 karaktera, veliko i malo slovo, broj i specijalni karakter.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Lozinke se ne poklapaju.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class AuthenticationResponseDto
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
