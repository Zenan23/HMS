using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<AuthenticationResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpiration(token);

            return new AuthenticationResponseDto
            {
                UserId = user.Id,
                Token = token,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthenticationResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user already exists
            if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            if (await _userRepository.ExistsByUsernameAsync(registerDto.Username))
            {
                throw new InvalidOperationException("User with this username already exists.");
            }

            // Create new user
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = _passwordService.HashPassword(registerDto.Password),
                Role = UserRole.Guest, // Default role
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpiration(token);

            return new AuthenticationResponseDto
            {
                UserId = user.Id,
                Token = token,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userRepository.ExistsByEmailAsync(email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _userRepository.ExistsByUsernameAsync(username);
        }
    }

}
