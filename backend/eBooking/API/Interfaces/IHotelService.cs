using API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IHotelService : IBaseService<HotelDto, CreateHotelDto, UpdateHotelDto>
    {
        Task<IEnumerable<HotelDto>> GetAllHotelsAsync();
        Task<HotelDto?> GetHotelByIdAsync(int id);
        Task<IEnumerable<HotelDto>> GetHotelsByCityAsync(string city);
        Task<HotelDto> CreateHotelAsync(CreateHotelDto createHotelDto);
        Task<bool> UpdateHotelAsync(int id, UpdateHotelDto updateHotelDto);
        Task<bool> DeleteHotelAsync(int id);
        Task<double> GetAverageRatingAsync(int hotelId);
        Task<IEnumerable<HotelDto>> GetUserBasedHotelRecommendationsAsync(int userId);
    }

}
