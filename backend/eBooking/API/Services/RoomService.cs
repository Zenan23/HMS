using API.DTOs;
using API.Enums;
using API.Interfaces;
using API.Models;
using API.Queries;
using AutoMapper;

namespace API.Services
{
    public class RoomService : BaseDtoService<Room, RoomDto, CreateRoomDto, UpdateRoomDto>, IRoomService
    {
        private readonly IBookingQueries _bookingQueries;
        private readonly IServiceQueries _serviceQueries;

        public RoomService(
            IRepository<Room> repository,
            IBookingQueries bookingQueries,
            IServiceQueries serviceQueries,
            IMapper mapper,
            ILogger<RoomService> logger)
            : base(repository, mapper, logger)
        {
            _bookingQueries = bookingQueries;
            _serviceQueries = serviceQueries;
    }

        public async Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(int hotelId)
        {
            try
            {
                _logger.LogInformation("Getting rooms for hotel ID: {HotelId}", hotelId);
                var rooms = await _repository.GetAllAsync();
                return rooms.Where(r => r.HotelId == hotelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms for hotel ID: {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                _logger.LogInformation("Getting available rooms for hotel {HotelId} from {CheckIn} to {CheckOut}",
                    hotelId, checkIn, checkOut);

                var rooms = await GetRoomsByHotelIdAsync(hotelId);
                var availableRooms = new List<Room>();

                foreach (var room in rooms.Where(r => r.IsAvailable))
                {
                    if (await IsRoomAvailableAsync(room.Id, checkIn, checkOut))
                    {
                        availableRooms.Add(room);
                    }
                }

                return availableRooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available rooms for hotel {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<IEnumerable<Room>> GetRoomsByTypeAsync(RoomType roomType)
        {
            try
            {
                _logger.LogInformation("Getting rooms by type: {RoomType}", roomType);
                var rooms = await _repository.GetAllAsync();
                return rooms.Where(r => r.RoomType == roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms by type: {RoomType}", roomType);
                throw;
            }
        }

        public async Task<IEnumerable<Room>> GetRoomsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                _logger.LogInformation("Getting rooms by price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
                var rooms = await _repository.GetAllAsync();
                return rooms.Where(r => r.PricePerNight >= minPrice && r.PricePerNight <= maxPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms by price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
                throw;
            }
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                var conflictingBookings = await _bookingQueries.GetOverlappingActiveBookingsAsync(roomId, checkIn, checkOut);

                return !conflictingBookings.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room availability for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<bool> RoomNumberExistsInHotelAsync(string roomNumber, int hotelId)
        {
            try
            {
                var rooms = await GetRoomsByHotelIdAsync(hotelId);
                return rooms.Any(r => r.RoomNumber.Equals(roomNumber, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room number exists: {RoomNumber} in hotel {HotelId}", roomNumber, hotelId);
                throw;
            }
        }

        public async Task<decimal> CalculatePriceAsync(int roomId, DateTime checkIn, DateTime checkOut, int guests)
        {
            var room = await _repository.GetByIdAsync(roomId);
            if (room == null)
                throw new InvalidOperationException("Room not found.");
            if (checkIn >= checkOut)
                throw new ArgumentException("Check-in date must be before check-out date.");
            if (guests <= 0)
                throw new ArgumentException("Number of guests must be at least 1.");

            var nights = (checkOut.Date - checkIn.Date).Days;
            if (nights <= 0)
                throw new ArgumentException("Stay must be at least one night.");

            // Osnovna kalkulacija: broj noćenja * cijena po noći
            decimal total = nights * room.PricePerNight;
            // Ovdje možeš dodati dodatne logike za popuste, takse, više gostiju itd.
            return total;
        }

        public async Task<decimal> CalculatePriceAsync(int roomId, DateTime checkIn, DateTime checkOut, int guests, IEnumerable<(int ServiceId, int Quantity)> services)
        {
            var baseTotal = await CalculatePriceAsync(roomId, checkIn, checkOut, guests);

            if (services == null)
                return baseTotal;

            var room = await _repository.GetByIdAsync(roomId) ?? throw new InvalidOperationException("Room not found.");
            decimal servicesTotal = 0m;
            foreach (var item in services)
            {
                var svc = await _serviceQueries.GetByIdAsync(item.ServiceId);
                if (svc == null || !svc.IsAvailable)
                    continue; // ili baci grešku po potrebi
                if (svc.HotelId != room.HotelId)
                    throw new InvalidOperationException("Service does not belong to the room's hotel.");
                var qty = item.Quantity <= 0 ? 1 : item.Quantity;
                servicesTotal += svc.Price * qty;
            }
            return baseTotal + servicesTotal;
        }

        public override async Task<RoomDto> CreateAsync(CreateRoomDto createDto)
        {
            if (await RoomNumberExistsInHotelAsync(createDto.RoomNumber, createDto.HotelId))
            {
                throw new InvalidOperationException($"Room number {createDto.RoomNumber} already exists in this hotel.");
            }

            return await base.CreateAsync(createDto);
        }

        public override async Task<bool> UpdateAsync(int id, UpdateRoomDto updateDto)
        {
            var existingRoom = await _repository.GetByIdAsync(id);
            if (existingRoom != null &&
                (!existingRoom.RoomNumber.Equals(updateDto.RoomNumber, StringComparison.OrdinalIgnoreCase) ||
                 existingRoom.HotelId != updateDto.HotelId))
            {
                if (await RoomNumberExistsInHotelAsync(updateDto.RoomNumber, updateDto.HotelId))
                {
                    throw new InvalidOperationException($"Room number {updateDto.RoomNumber} already exists in this hotel.");
                }
            }

            return await base.UpdateAsync(id, updateDto);
        }
    }
}
