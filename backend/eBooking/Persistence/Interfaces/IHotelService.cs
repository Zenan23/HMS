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
        Task<IEnumerable<HotelDto>> GetUserBasedHotelRecommendationsAsync(int userId, int maxRecommendations = 3);
        Task<HotelStatistics> GetHotelStatisticsAsync();
        Task<IEnumerable<HotelDto>> GetHotelsByNameAsync(string name);
        Task<HotelDto?> SetHotelImageAsync(int hotelId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
        Task<bool> RemoveHotelImageAsync(int hotelId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Evidentira da je korisnik pregledao detalje hotela — stvarni ponašajni signal koji se
        /// koristi u sistemu preporuke (popularity-based komponenta), ne samo prikuplja bez svrhe.
        /// Best-effort: greška pri upisu ne smije srušiti prikaz hotela korisniku.
        /// </summary>
        Task RecordHotelViewAsync(int userId, int hotelId);
    }
}
