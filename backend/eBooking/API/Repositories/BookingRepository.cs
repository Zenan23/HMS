using API.Data;
using API.Enums;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public override async Task<Booking?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByGuestAsync(int guestId)
        {
            return await _dbSet
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .Include(b => b.User)
                .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
                .Where(b => b.UserId == guestId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByRoomAsync(int roomId)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
                .Where(b => b.RoomId == roomId)
                .OrderByDescending(b => b.CheckInDate)
                .ToListAsync();
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var overlappingBookings = await _dbSet
                .Where(b => b.RoomId == roomId &&
                           b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.CheckedOut &&
                           ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                            (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                            (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)))
                .AnyAsync();

            return !overlappingBookings;
        }
    }

}
