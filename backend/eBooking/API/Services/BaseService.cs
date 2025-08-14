using API.Interfaces;
using API.Models;

namespace API.Services
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity
    {
        protected readonly IRepository<T> _repository;
        protected readonly ILogger<BaseService<T>> _logger;

        public BaseService(IRepository<T> repository, ILogger<BaseService<T>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting {EntityType} with ID: {Id}", typeof(T).Name, id);
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} with ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all {EntityType} entities", typeof(T).Name);
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting {EntityType} entities - Page: {PageNumber}, Size: {PageSize}",
                    typeof(T).Name, pageNumber, pageSize);

                var skip = (pageNumber - 1) * pageSize;
                var allEntities = await _repository.GetAllAsync();
                return allEntities.Skip(skip).Take(pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged {EntityType} entities", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                _logger.LogInformation("Adding new {EntityType} entity", typeof(T).Name);

                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                var result = await _repository.AddAsync(entity);
                _logger.LogInformation("Successfully added {EntityType} with ID: {Id}", typeof(T).Name, result.Id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {EntityType} entity", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                _logger.LogInformation("Updating {EntityType} with ID: {Id}", typeof(T).Name, entity.Id);

                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingEntity = await _repository.GetByIdAsync(entity.Id);
                if (existingEntity == null)
                    throw new InvalidOperationException($"{typeof(T).Name} with ID {entity.Id} not found");

                entity.UpdatedAt = DateTime.UtcNow;
                entity.CreatedAt = existingEntity.CreatedAt; // Preserve original creation date

                await _repository.UpdateAsync(entity);
                _logger.LogInformation("Successfully updated {EntityType} with ID: {Id}", typeof(T).Name, entity.Id);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType} with ID: {Id}", typeof(T).Name, entity.Id);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting {EntityType} with ID: {Id}", typeof(T).Name, id);

                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("{EntityType} with ID {Id} not found for deletion", typeof(T).Name, id);
                    return false;
                }

                // Soft delete
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                _logger.LogInformation("Successfully deleted {EntityType} with ID: {Id}", typeof(T).Name, id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                return entity != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of {EntityType} with ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                return entities.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting {EntityType} entities", typeof(T).Name);
                throw;
            }
        }
    }
}
