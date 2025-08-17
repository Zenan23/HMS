using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public abstract class BaseDtoService<TEntity, TDto, TCreateDto, TUpdateDto> :
        IBaseService<TDto, TCreateDto, TUpdateDto>
        where TEntity : BaseEntity
        where TDto : BaseEntityDto
        where TCreateDto : CreateBaseEntityDto
        where TUpdateDto : UpdateBaseEntityDto
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ILogger<BaseDtoService<TEntity, TDto, TCreateDto, TUpdateDto>> _logger;

        protected BaseDtoService(
            IRepository<TEntity> repository,
            IMapper mapper,
            ILogger<BaseDtoService<TEntity, TDto, TCreateDto, TUpdateDto>> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public virtual async Task<TDto?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                var entity = await _repository.GetByIdAsync(id);
                return entity == null ? null : _mapper.Map<TDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all {EntityType} entities", typeof(TEntity).Name);
                var entities = await _repository.GetAllAsync();
                return _mapper.Map<IEnumerable<TDto>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting {EntityType} entities - Page: {PageNumber}, Size: {PageSize}",
                    typeof(TEntity).Name, pageNumber, pageSize);

                var skip = (pageNumber - 1) * pageSize;
                var allEntities = await _repository.GetAllAsync();
                var pagedEntities = allEntities.Skip(skip).Take(pageSize);
                return _mapper.Map<IEnumerable<TDto>>(pagedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged {EntityType} entities", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<TDto> CreateAsync(TCreateDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating new {EntityType} entity", typeof(TEntity).Name);

                if (createDto == null)
                    throw new ArgumentNullException(nameof(createDto));

                var entity = _mapper.Map<TEntity>(createDto);
                var createdEntity = await _repository.AddAsync(entity);

                _logger.LogInformation("Successfully created {EntityType} with ID: {Id}", typeof(TEntity).Name, createdEntity.Id);

                return _mapper.Map<TDto>(createdEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {EntityType} entity", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<bool> UpdateAsync(int id, TUpdateDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating {EntityType} with ID: {Id}", typeof(TEntity).Name, id);

                if (updateDto == null)
                    throw new ArgumentNullException(nameof(updateDto));

                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                {
                    _logger.LogWarning("{EntityType} with ID {Id} not found for update", typeof(TEntity).Name, id);
                    return false;
                }

                _mapper.Map(updateDto, existingEntity);
                await _repository.UpdateAsync(existingEntity);

                _logger.LogInformation("Successfully updated {EntityType} with ID: {Id}", typeof(TEntity).Name, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting {EntityType} with ID: {Id}", typeof(TEntity).Name, id);

                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("{EntityType} with ID {Id} not found for deletion", typeof(TEntity).Name, id);
                    return false;
                }

                // Soft delete
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                _logger.LogInformation("Successfully deleted {EntityType} with ID: {Id}", typeof(TEntity).Name, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                return entity != null && !entity.IsDeleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                return entities.Where(e => !e.IsDeleted).Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting {EntityType} entities", typeof(TEntity).Name);
                throw;
            }
        }
    }
}
