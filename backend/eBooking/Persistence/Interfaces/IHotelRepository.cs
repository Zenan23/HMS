using Persistence.Models;

namespace Persistence.Interfaces
{
    public interface IHotelRepository : IRepository<Hotel>
    {
        Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city);
        Task<Hotel?> GetHotelWithRoomsAsync(int hotelId);
    }
}
