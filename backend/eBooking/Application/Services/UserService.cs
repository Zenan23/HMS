using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class UserService : BaseDtoService<User, UserDto, CreateUserDto, UpdateUserDto>, IUserService
    {
        private readonly IPasswordService _passwordService;

        public UserService(
            IRepository<User> repository,
            IMapper mapper,
            ILogger<UserService> logger,
            IPasswordService passwordService)
            : base(repository, mapper, logger)
        {
            _passwordService = passwordService;
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            try
            {
                _logger.LogInformation("Getting user by username: {Username}", username);
                var entities = await _repository.GetAllAsync();
                var user = entities.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted);
                return user == null ? null : _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task<UserDto?> GetEmployeeByUsernameAsync(string username)
        {
            try
            {
                _logger.LogInformation("Getting user by username: {Username}", username);
                var entities = await _repository.GetAllAsync();
                var user = entities.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted && u.Role == UserRole.Employee);
                return user == null ? null : _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Getting user by email: {Email}", email);
                var entities = await _repository.GetAllAsync();
                var user = entities.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && !u.IsDeleted);
                return user == null ? null : _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetByRoleAsync(int role)
        {
            try
            {
                _logger.LogInformation("Getting users by role: {Role}", role);
                var entities = await _repository.GetAllAsync();
                if (Enum.IsDefined(typeof(UserRole), role))
                {
                    var parsedRole = (UserRole)role;
                    var filteredEntities = entities
                        .Where(u => u.Role == parsedRole && !u.IsDeleted);
                    return _mapper.Map<IEnumerable<UserDto>>(filteredEntities);

                }

                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role: {Role}", role);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
        {
            try
            {
                _logger.LogInformation("Getting active users");
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(u => u.IsActive && !u.IsDeleted);
                return _mapper.Map<IEnumerable<UserDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users");
                throw;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            try
            {
                _logger.LogInformation("Updating password for user ID: {UserId}", userId);

                var user = await _repository.GetByIdAsync(userId);
                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning("User with ID {UserId} not found for password update", userId);
                    return false;
                }

                user.PasswordHash = _passwordService.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(user);
                _logger.LogInformation("Successfully updated password for user ID: {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user ID: {UserId}", userId);
                throw;
            }
        }

        public override async Task<UserDto> CreateAsync(CreateUserDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating new user with username: {Username}", createDto.Username);

                // Check if username or email already exists
                var existingByUsername = await GetByUsernameAsync(createDto.Username);
                if (existingByUsername != null)
                {
                    throw new InvalidOperationException($"Username '{createDto.Username}' already exists");
                }

                var existingByEmail = await GetByEmailAsync(createDto.Email);
                if (existingByEmail != null)
                {
                    throw new InvalidOperationException($"Email '{createDto.Email}' already exists");
                }

                var user = _mapper.Map<User>(createDto);
                user.PasswordHash = _passwordService.HashPassword(createDto.Password);

                var createdUser = await _repository.AddAsync(user);
                _logger.LogInformation("Successfully created user with ID: {Id}", createdUser.Id);

                return _mapper.Map<UserDto>(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with username: {Username}", createDto.Username);
                throw;
            }
        }

        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var activeUsers = entities.Where(u => !u.IsDeleted).ToList();

                var totalUsers = activeUsers.Count;
                var activeUserCount = activeUsers.Count(u => u.IsActive);
                var guestUsers = activeUsers.Count(u => u.Role == UserRole.Guest);
                var employeeUsers = activeUsers.Count(u => u.Role == UserRole.Employee);
                var adminUsers = activeUsers.Count(u => u.Role == UserRole.Admin);

                // New users this month
                var thisMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                var newUsersThisMonth = activeUsers.Count(u => u.CreatedAt >= thisMonth);

                // Monthly data for last 12 months
                var monthlyData = new List<MonthlyUserData>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.UtcNow.AddMonths(-i).Date.AddDays(1 - DateTime.UtcNow.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    
                    var monthQuery = activeUsers.Where(u => u.CreatedAt >= monthStart && u.CreatedAt <= monthEnd);
                    var newUsers = monthQuery.Count();
                    var activeUsersInMonth = monthQuery.Count(u => u.IsActive);

                    monthlyData.Add(new MonthlyUserData
                    {
                        Month = monthStart.ToString("MMM yyyy"),
                        NewUsers = newUsers,
                        ActiveUsers = activeUsersInMonth
                    });
                }

                return new UserStatistics
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUserCount,
                    NewUsersThisMonth = newUsersThisMonth,
                    GuestUsers = guestUsers,
                    EmployeeUsers = employeeUsers,
                    AdminUsers = adminUsers,
                    MonthlyData = monthlyData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user statistics");
                throw;
            }
        }
    }
}
