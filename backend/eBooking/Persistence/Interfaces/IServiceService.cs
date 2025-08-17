using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IServiceService : IBaseService<ServiceDto, CreateServiceDto, UpdateServiceDto>
    {
        Task<IEnumerable<ServiceDto>> GetByHotelIdAsync(int hotelId);
        Task<IEnumerable<ServiceDto>> GetByCategoryAsync(string category);
        Task<IEnumerable<ServiceDto>> GetAvailableServicesAsync();
    }
}
