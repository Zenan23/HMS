using API.DTOs;

namespace API.Interfaces
{
    public interface IServiceService : IBaseService<ServiceDto, CreateServiceDto, UpdateServiceDto>
    {
        Task<IEnumerable<ServiceDto>> GetByHotelIdAsync(int hotelId);
        Task<IEnumerable<ServiceDto>> GetByCategoryAsync(string category);
        Task<IEnumerable<ServiceDto>> GetAvailableServicesAsync();
    }
}
