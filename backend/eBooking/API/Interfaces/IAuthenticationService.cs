using API.DTOs;

namespace API.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthenticationResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }
}
