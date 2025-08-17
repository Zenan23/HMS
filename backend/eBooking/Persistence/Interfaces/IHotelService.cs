using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IHotelService : IBaseService<HotelDto, CreateHotelDto, UpdateHotelDto>
    {
        Task<IEnumerable<HotelDto>> GetAllHotelsAsync(int? rating = null, string city = null, string name = null);
        Task<HotelDto?> GetHotelByIdAsync(int id);
        Task<IEnumerable<HotelDto>> GetHotelsByCityAsync(string city);
        Task<HotelDto> CreateHotelAsync(CreateHotelDto createHotelDto);
        Task<bool> UpdateHotelAsync(int id, UpdateHotelDto updateHotelDto);
        Task<bool> DeleteHotelAsync(int id);
        Task<double> GetAverageRatingAsync(int hotelId);
        Task<IEnumerable<HotelDto>> GetUserBasedHotelRecommendationsAsync(int userId);
        Task<HotelStatistics> GetHotelStatisticsAsync();
        Task<IEnumerable<HotelDto>> GetHotelsByNameAsync(string name);
    }

}
