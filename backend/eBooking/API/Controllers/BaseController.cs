using System.Security.Claims;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController<TDto, TCreateDto, TUpdateDto> : ControllerBase
        where TDto : BaseEntityDto
        where TCreateDto : CreateBaseEntityDto
        where TUpdateDto : UpdateBaseEntityDto
    {
        protected readonly IBaseService<TDto, TCreateDto, TUpdateDto> _service;
        protected readonly ILogger<BaseController<TDto, TCreateDto, TUpdateDto>> _logger;

        protected BaseController(
            IBaseService<TDto, TCreateDto, TUpdateDto> service,
            ILogger<BaseController<TDto, TCreateDto, TUpdateDto>> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Provjera da li ulogovani korisnik smije pristupiti podacima korisnika sa datim ID-jem —
        /// dozvoljeno ako je to on sam (podudaranje sa JWT claim-om), ili ako je Employee/Admin.
        /// Koristi se na endpoint-ima tipa GET .../user/{userId} da se spriječi IDOR
        /// (npr. bilo koji ulogovani korisnik da vidi tuđe rezervacije/uplate/recenzije).
        /// </summary>
        protected bool IsSelfOrElevated(int userId)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null &&
                (roleClaim.Value == UserRole.Employee.ToString() || roleClaim.Value == UserRole.Admin.ToString()))
            {
                return true;
            }

            var uidClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return uidClaim != null && int.TryParse(uidClaim.Value, out var callerId) && callerId == userId;
        }

        protected ActionResult<ApiResponse<TDto>>? MapServiceException(Exception ex, string operation)
        {
            switch (ex)
            {
                case ArgumentException:
                    return BadRequest(ApiResponse<TDto>.ErrorResult(ex.Message));
                case InvalidOperationException:
                    return Conflict(ApiResponse<TDto>.ErrorResult(ex.Message));
                case KeyNotFoundException:
                    return NotFound(ApiResponse<TDto>.ErrorResult(ex.Message));
                default:
                    return null;
            }
        }

        protected ActionResult<ApiResponse<bool>>? MapBoolServiceException(Exception ex)
        {
            switch (ex)
            {
                case ArgumentException:
                    return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
                case InvalidOperationException:
                    return Conflict(ApiResponse<bool>.ErrorResult(ex.Message));
                case KeyNotFoundException:
                    return NotFound(ApiResponse<bool>.ErrorResult(ex.Message));
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity if found</returns>
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<ApiResponse<TDto>>> GetById([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Neispravan ID. ID mora biti veći od 0."));
                }

                var dto = await _service.GetByIdAsync(id);
                if (dto == null)
                {
                    return NotFound(ApiResponse<TDto>.ErrorResult($"Zapis sa ID {id} nije pronađen."));
                }

                return Ok(ApiResponse<TDto>.SuccessResult(dto, "Zapis je uspješno učitan."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity with ID: {Id}", id);
                return StatusCode(500, ApiResponse<TDto>.ErrorResult("Došlo je do greške pri učitavanju zapisa."));
            }
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>List of entities</returns>
        [HttpGet]
        public virtual async Task<ActionResult<ApiResponse<PaginatedResult<TDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Limit page size

                var dtos = await _service.GetAllAsync(pageNumber, pageSize);
                var totalCount = await _service.CountAsync();

                var paginatedResult = new PaginatedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(ApiResponse<PaginatedResult<TDto>>.SuccessResult(
                    paginatedResult,
                    "Zapisi su uspješno učitani."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entities");
                return StatusCode(500, ApiResponse<PaginatedResult<TDto>>.ErrorResult("Došlo je do greške pri učitavanju zapisa."));
            }
        }

        /// <summary>
        /// Create a new entity
        /// </summary>
        /// <param name="createDto">Entity creation data</param>
        /// <returns>Created entity</returns>
        [HttpPost]
        public virtual async Task<ActionResult<ApiResponse<TDto>>> Create([FromBody] TCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Validacija nije uspjela.", errors));
                }

                var dto = await _service.CreateAsync(createDto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = dto.Id },
                    ApiResponse<TDto>.SuccessResult(dto, "Zapis je uspješno kreiran."));
            }
            catch (Exception ex)
            {
                var mapped = MapServiceException(ex, "create");
                if (mapped != null)
                    return mapped;

                _logger.LogError(ex, "Error creating entity");
                return StatusCode(500, ApiResponse<TDto>.ErrorResult("Došlo je do greške pri kreiranju zapisa."));
            }
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="updateDto">Entity update data</param>
        /// <returns>Updated entity</returns>
        [HttpPut("{id}")]
        public virtual async Task<ActionResult<ApiResponse<TDto>>> Update([FromRoute] int id, [FromBody] TUpdateDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Neispravan ID. ID mora biti veći od 0."));
                }

                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<TDto>.ErrorResult("ID u URL-u se ne poklapa sa ID-om u tijelu zahtjeva."));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Validacija nije uspjela.", errors));
                }

                var result = await _service.UpdateAsync(id, updateDto);
                if (!result)
                {
                    return NotFound(ApiResponse<TDto>.ErrorResult($"Zapis sa ID {id} nije pronađen."));
                }

                var updatedDto = await _service.GetByIdAsync(id);
                return Ok(ApiResponse<TDto>.SuccessResult(updatedDto!, "Zapis je uspješno ažuriran."));
            }
            catch (Exception ex)
            {
                var mapped = MapServiceException(ex, "update");
                if (mapped != null)
                    return mapped;

                _logger.LogError(ex, "Error updating entity with ID: {Id}", id);
                return StatusCode(500, ApiResponse<TDto>.ErrorResult("Došlo je do greške pri ažuriranju zapisa."));
            }
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Neispravan ID. ID mora biti veći od 0."));
                }

                var exists = await _service.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"Zapis sa ID {id} nije pronađen."));
                }

                var result = await _service.DeleteAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Zapis je uspješno obrisan."));
            }
            catch (Exception ex)
            {
                var mapped = MapBoolServiceException(ex);
                if (mapped != null)
                    return mapped;

                _logger.LogError(ex, "Error deleting entity with ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Došlo je do greške pri brisanju zapisa."));
            }
        }

        /// <summary>
        /// Check if entity exists
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Existence check result</returns>
        [HttpHead("{id}")]
        public virtual async Task<ActionResult> Exists([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest();
                }

                var exists = await _service.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of entity with ID: {Id}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get entity count
        /// </summary>
        /// <returns>Total count of entities</returns>
        [HttpGet("count")]
        public virtual async Task<ActionResult<ApiResponse<int>>> GetCount()
        {
            try
            {
                var count = await _service.CountAsync();
                return Ok(ApiResponse<int>.SuccessResult(count, "Total entity count retrieved."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity count");
                return StatusCode(500, ApiResponse<int>.ErrorResult("An error occurred while getting entity count."));
            }
        }
    }
}
