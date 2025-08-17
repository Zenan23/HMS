using Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : BaseController<UserDto, CreateUserDto, UpdateUserDto>
    {
        private readonly IUserService _userService;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
            : base(userService, logger)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found</returns>
        [HttpGet("username/{username}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetByUsername([FromRoute] string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResult("Username is required."));
                }

                var user = await _userService.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult($"User with username '{username}' not found."));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user, "User retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return StatusCode(500, ApiResponse<UserDto>.ErrorResult("An error occurred while retrieving user."));
            }
        }


        /// <summary>
        /// Get employee by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found</returns>
        [HttpGet("employee/username/{username}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetEmployeeByUsername([FromRoute] string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResult("Username is required."));
                }

                var user = await _userService.GetEmployeeByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult($"User with username '{username}' not found."));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user, "User retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return StatusCode(500, ApiResponse<UserDto>.ErrorResult("An error occurred while retrieving user."));
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User if found</returns>
        [HttpGet("email/{email}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetByEmail([FromRoute] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResult("Email is required."));
                }

                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult($"User with email '{email}' not found."));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user, "User retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return StatusCode(500, ApiResponse<UserDto>.ErrorResult("An error occurred while retrieving user."));
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns>List of users</returns>
        [HttpGet("role/{role}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetByRole([FromRoute] int role)
        {
            try
            {
                var users = await _userService.GetByRoleAsync(role);
                return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResult(users, "Users retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users by role: {Role}", role);
                return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResult("An error occurred while retrieving users."));
            }
        }

        /// <summary>
        /// Get active users
        /// </summary>
        /// <returns>List of active users</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResult(users, "Active users retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users");
                return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResult("An error occurred while retrieving active users."));
            }
        }

        /// <summary>
        /// Update user password
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Password update request</param>
        /// <returns>Update result</returns>
        [HttpPatch("{id}/password")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdatePassword([FromRoute] int id, [FromBody] UpdatePasswordRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid user ID."));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<bool>.ErrorResult("Validation failed.", errors));
                }

                var result = await _userService.UpdatePasswordAsync(id, request.NewPassword);
                if (!result)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"User with ID {id} not found."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Password updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user ID: {UserId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while updating password."));
            }
        }
    }

    public class UpdatePasswordRequest
    {
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
