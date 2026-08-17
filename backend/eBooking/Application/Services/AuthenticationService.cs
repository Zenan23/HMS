using System.Security.Cryptography;
using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
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
        private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthenticationService> _logger;

        private const int ResetCodeValidityMinutes = 15;

        public AuthenticationService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IMapper mapper,
            IRepository<PasswordResetToken> passwordResetTokenRepository,
            IEmailService emailService,
            ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _mapper = mapper;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
            _logger = logger;
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
                throw new InvalidOperationException("Korisnik sa ovim email-om vec postoji u bazi.");
            }

            if (await _userRepository.ExistsByUsernameAsync(registerDto.Username))
            {
                throw new InvalidOperationException("Korisnik sa ovim korisničkim imenom vec postoji u bazi.");
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

        public async Task ForgotPasswordAsync(string email)
        {
            // Namjerno se NE otkriva da li email postoji — spriječi enumeraciju korisnika.
            // Uvijek "uspješno" vraća pozivaocu, ali kod se generiše i šalje samo ako korisnik
            // stvarno postoji.
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                _logger.LogInformation("Forgot-password zahtjev za nepostojeći/neaktivan email {Email} — ignorisano.", email);
                return;
            }

            // 6-cifreni kod generisan preko RandomNumberGenerator (ne System.Random) — Dodatak A.3.
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var codeHash = _passwordService.HashPassword(code);

            await _passwordResetTokenRepository.AddAsync(new PasswordResetToken
            {
                UserId = user.Id,
                CodeHash = codeHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ResetCodeValidityMinutes),
                Used = false,
                CreatedAt = DateTime.UtcNow
            });

            var body = $@"
                <p>Poštovani/a {user.FirstName},</p>
                <p>Vaš kod za resetovanje lozinke je:</p>
                <h2>{code}</h2>
                <p>Kod važi {ResetCodeValidityMinutes} minuta. Ako niste vi zatražili reset lozinke, slobodno ignorišite ovaj email.</p>";

            await _emailService.SendEmailAsync(user.Email, "eBooking — reset lozinke", body);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            var tokens = await _passwordResetTokenRepository.FindAsync(t =>
                t.UserId == user.Id && !t.Used && t.ExpiresAt > DateTime.UtcNow);

            // Najnoviji nekorišteni, neistekli kod za ovog korisnika.
            var token = tokens.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
            if (token == null || !_passwordService.VerifyPassword(dto.Code, token.CodeHash))
            {
                return false;
            }

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            token.Used = true;
            await _passwordResetTokenRepository.UpdateAsync(token);

            _logger.LogInformation("Lozinka uspješno resetovana za korisnika {UserId}", user.Id);
            return true;
        }
    }

}
