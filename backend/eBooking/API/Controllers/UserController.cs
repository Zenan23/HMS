using API.Attributes;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

        // Lista SVIH korisnika / pojedinačan korisnik po ID-u — samo osoblje/admin (PII).
        // Sopstveni profil se dohvata iz AuthService (JWT), ne preko ovih endpoint-a.
        [HttpGet]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<PaginatedResult<UserDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => base.GetAll(pageNumber, pageSize);

        [HttpGet("{id}")]
        public override async Task<ActionResult<ApiResponse<UserDto>>> GetById([FromRoute] int id)
        {
            if (!IsSelfOrElevated(id))
            {
                return Forbid();
            }
            return await base.GetById(id);
        }

        /// <summary>
        /// Ažuriranje korisnika — admin može ažurirati bilo kog korisnika (uključujući rolu/status).
        /// Ne-admin korisnik smije ažurirati samo svoj profil i ne može mijenjati vlastitu rolu/status.
        /// </summary>
        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<UserDto>>> Update([FromRoute] int id, [FromBody] UpdateUserDto updateDto)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var isAdmin = roleClaim != null && roleClaim.Value == UserRole.Admin.ToString();

            if (!isAdmin)
            {
                var uidClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (uidClaim == null || !int.TryParse(uidClaim.Value, out var callerId) || callerId != id)
                {
                    return Forbid();
                }

                // Spriječi da korisnik sam sebi promijeni rolu ili status aktivnosti kroz svoj profil.
                var existing = await _userService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult($"Korisnik sa ID {id} nije pronađen."));
                }
                updateDto.Role = existing.Role;
                updateDto.IsActive = existing.IsActive;
            }

            return await base.Update(id, updateDto);
        }

        /// <summary>
        /// Brisanje korisnika — samo Admin.
        /// </summary>
        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Admin)]
        public override async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
        {
            return await base.Delete(id);
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found</returns>
        [HttpGet("username/{username}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetByUsername([FromRoute] string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResult("Korisničko ime je obavezno."));
                }

                var user = await _userService.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult($"Korisnik sa korisničkim imenom '{username}' nije pronađen."));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user, "Korisnik je uspješno učitan."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Došlo je do greške pri učitavanju korisnika."));
            }
        }


        /// <summary>
        /// Get employee by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User if found</returns>
        [HttpGet("employee/username/{username}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetEmployeeByUsername([FromRoute] string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResult("Korisničko ime je obavezno."));
                }

                var user = await _userService.GetEmployeeByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult($"Korisnik sa korisničkim imenom '{username}' nije pronađen."));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user, "Korisnik je uspješno učitan."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Došlo je do greške pri učitavanju korisnika."));
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User if found</returns>
        [HttpGet("email/{email}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetByEmail([FromRoute] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResult("Email je obavezan."));
                }

                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult($"Korisnik sa email-om '{email}' nije pronađen."));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user, "Korisnik je uspješno učitan."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Došlo je do greške pri učitavanju korisnika."));
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns>List of users</returns>
        [HttpGet("role/{role}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetByRole([FromRoute] int role)
        {
            try
            {
                var users = await _userService.GetByRoleAsync(role);
                return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResult(users, "Korisnici su uspješno učitani."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users by role: {Role}", role);
                return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResult("Došlo je do greške pri učitavanju korisnika."));
            }
        }

        /// <summary>
        /// Get active users
        /// </summary>
        /// <returns>List of active users</returns>
        [HttpGet("active")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResult(users, "Aktivni korisnici su uspješno učitani."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users");
                return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResult("Došlo je do greške pri učitavanju aktivnih korisnika."));
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
                    return BadRequest(ApiResponse<bool>.ErrorResult("Nevažeći ID korisnika."));
                }

                if (!IsSelfOrElevated(id))
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<bool>.ErrorResult("Validacija nije uspjela.", errors));
                }

                var result = await _userService.UpdatePasswordAsync(id, request.NewPassword);
                if (!result)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"Korisnik sa ID {id} nije pronađen."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Lozinka je uspješno promijenjena."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for user ID: {UserId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Došlo je do greške pri promjeni lozinke."));
            }
        }
    }

    public class UpdatePasswordRequest
    {
        [Required(ErrorMessage = "Nova lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje 6 karaktera.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
