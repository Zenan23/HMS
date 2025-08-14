using API.DTOs;

namespace API.Interfaces
{
    public interface IUserService : IBaseService<UserDto, CreateUserDto, UpdateUserDto>
    {
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<UserDto?> GetByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetByRoleAsync(int role);
        Task<IEnumerable<UserDto>> GetActiveUsersAsync();
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
    }
}
