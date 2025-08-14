using API.DTOs;
using API.Enums;
using API.Models;

namespace API.Interfaces
{
    public interface IRoomService : IBaseService<RoomDto, CreateRoomDto, UpdateRoomDto>
    {
        Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(int hotelId);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<Room>> GetRoomsByTypeAsync(RoomType roomType);
        Task<IEnumerable<Room>> GetRoomsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<bool> RoomNumberExistsInHotelAsync(string roomNumber, int hotelId);
        Task<decimal> CalculatePriceAsync(int roomId, DateTime checkIn, DateTime checkOut, int guests);
        Task<decimal> CalculatePriceAsync(int roomId, DateTime checkIn, DateTime checkOut, int guests, IEnumerable<(int ServiceId, int Quantity)> services);
    }
}
