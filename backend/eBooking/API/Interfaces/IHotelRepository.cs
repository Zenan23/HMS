using API.Models;

namespace API.Interfaces
{
    public interface IHotelRepository : IRepository<Hotel>
    {
        Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city);
        Task<Hotel?> GetHotelWithRoomsAsync(int hotelId);
    }
}
