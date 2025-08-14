using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("/api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
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
