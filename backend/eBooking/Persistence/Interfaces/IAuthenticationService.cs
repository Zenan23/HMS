using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthenticationResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Pokreće reset lozinke: ako korisnik sa datim emailom postoji, generiše 6-cifreni kod
        /// (hashovan, sa istekom) i šalje ga emailom. UVIJEK "uspješno" završava bez obzira da li
        /// email postoji u bazi — sprječava enumeraciju registrovanih korisnika.
        /// </summary>
        Task ForgotPasswordAsync(string email);

        /// <summary>Postavlja novu lozinku ako je kod ispravan, nekorišten i nije istekao.</summary>
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
