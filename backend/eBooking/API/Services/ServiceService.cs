using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;

namespace API.Services
{
    public class ServiceService : BaseDtoService<Service, ServiceDto, CreateServiceDto, UpdateServiceDto>, IServiceService
    {
        public ServiceService(
            IRepository<Service> repository,
            IMapper mapper,
            ILogger<ServiceService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<ServiceDto>> GetByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Getting services for hotel ID: {HotelId}", hotelId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(s => s.HotelId == hotelId && !s.IsDeleted);
                return _mapper.Map<IEnumerable<ServiceDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for hotel ID: {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<ServiceDto>> GetByCategoryAsync(string category)
        {
            try
            {
                _logger.LogInformation("Getting services for category: {Category}", category);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && !s.IsDeleted);
                return _mapper.Map<IEnumerable<ServiceDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for category: {Category}", category);
                throw;
            }
        }

        public async Task<IEnumerable<ServiceDto>> GetAvailableServicesAsync()
        {
            try
            {
                _logger.LogInformation("Getting available services");
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(s => s.IsAvailable && s.IsActive && !s.IsDeleted);
                return _mapper.Map<IEnumerable<ServiceDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available services");
                throw;
            }
        }
    }
}
