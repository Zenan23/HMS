using Contracts.DTOs;
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
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Invalid ID. ID must be greater than 0."));
                }

                var dto = await _service.GetByIdAsync(id);
                if (dto == null)
                {
                    return NotFound(ApiResponse<TDto>.ErrorResult($"Entity with ID {id} not found."));
                }

                return Ok(ApiResponse<TDto>.SuccessResult(dto, "Entity retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity with ID: {Id}", id);
                return StatusCode(500, ApiResponse<TDto>.ErrorResult("An error occurred while retrieving the entity."));
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
                    "Entities retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entities");
                return StatusCode(500, ApiResponse<PaginatedResult<TDto>>.ErrorResult("An error occurred while retrieving entities."));
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
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Validation failed.", errors));
                }

                var dto = await _service.CreateAsync(createDto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = dto.Id },
                    ApiResponse<TDto>.SuccessResult(dto, "Entity created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity");
                return StatusCode(500, ApiResponse<TDto>.ErrorResult("An error occurred while creating the entity."));
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
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Invalid ID. ID must be greater than 0."));
                }

                if (id != updateDto.Id)
                {
                    return BadRequest(ApiResponse<TDto>.ErrorResult("ID in URL does not match ID in request body."));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<TDto>.ErrorResult("Validation failed.", errors));
                }

                var result = await _service.UpdateAsync(id, updateDto);
                if (!result)
                {
                    return NotFound(ApiResponse<TDto>.ErrorResult($"Entity with ID {id} not found."));
                }

                var updatedDto = await _service.GetByIdAsync(id);
                return Ok(ApiResponse<TDto>.SuccessResult(updatedDto!, "Entity updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity with ID: {Id}", id);
                return StatusCode(500, ApiResponse<TDto>.ErrorResult("An error occurred while updating the entity."));
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
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid ID. ID must be greater than 0."));
                }

                var exists = await _service.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"Entity with ID {id} not found."));
                }

                var result = await _service.DeleteAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Entity deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity with ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while deleting the entity."));
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
