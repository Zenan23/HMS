using Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("/api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ITokenRevocationService _tokenRevocationService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authenticationService,
            ITokenRevocationService tokenRevocationService,
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _authenticationService = authenticationService;
            _tokenRevocationService = tokenRevocationService;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Odjava korisnika — poništava trenutni JWT token na serveru (upisuje jti u listu
        /// poništenih tokena) umjesto da se token samo lokalno obriše na klijentu.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Nevažeći token." });
                }

                var authHeader = Request.Headers.Authorization.ToString();
                var rawToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader["Bearer ".Length..]
                    : authHeader;

                var expiresAt = string.IsNullOrWhiteSpace(rawToken)
                    ? DateTime.UtcNow.AddDays(2) // sigurnosna margina ako token nije dostupan u header-u
                    : _jwtService.GetTokenExpiration(rawToken);

                await _tokenRevocationService.RevokeAsync(jti, userId, expiresAt);

                _logger.LogInformation("User {UserId} logged out, token {Jti} revoked", userId, jti);
                return Ok(new { message = "Odjava uspješna." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Došlo je do greške pri odjavi." });
            }
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authenticationService.LoginAsync(loginDto);
                _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed login attempt for {Email}: {Message}", loginDto.Email, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginDto.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// User registration
        /// </summary>
        /// <param name="registerDto">Registration information</param>
        /// <returns>Authentication token and user information</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authenticationService.RegisterAsync(registerDto);
                _logger.LogInformation("User {Email} registered successfully", registerDto.Email);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed for {Email}: {Message}", registerDto.Email, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", registerDto.Email);
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var user = await _authenticationService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "An error occurred while retrieving profile" });
            }
        }

        /// <summary>
        /// Check if email is available
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-email")]
        public async Task<ActionResult<object>> CheckEmailAvailability([FromQuery] string email)
        {
            try
            {
                var exists = await _authenticationService.UserExistsAsync(email);
                return Ok(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability");
                return StatusCode(500, new { message = "An error occurred while checking email availability" });
            }
        }

        /// <summary>
        /// Check if username is available
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-username")]
        public async Task<ActionResult<object>> CheckUsernameAvailability([FromQuery] string username)
        {
            try
            {
                var exists = await _authenticationService.UsernameExistsAsync(username);
                return Ok(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability");
                return StatusCode(500, new { message = "An error occurred while checking username availability" });
            }
        }
    }
}
