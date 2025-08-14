using API.DTOs;
using API.Enums;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using API.Contracts.Messages;

namespace API.Services
{
    public class BookingService : BaseDtoService<Booking, BookingDto, CreateBookingDto, UpdateBookingDto>, IBookingService
    {
        private readonly IBookingStatusHistoryService _bookingStatusHistoryService;
        private readonly IPaymentService _paymentService;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<API.Models.BookingService> _bookingServiceRepository;
        private readonly IRoomService _roomService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public BookingService(
            IRepository<Booking> repository,
            IMapper mapper,
            ILogger<BookingService> logger,
            IBookingStatusHistoryService bookingStatusHistoryService,
            IPaymentService paymentService,
            IRepository<Service> serviceRepository,
            IRepository<API.Models.BookingService> bookingServiceRepository,
            IRoomService roomService,
            IBookingRepository bookingRepository,
            IPublishEndpoint publishEndpoint)
            : base(repository, mapper, logger)
        {
            _bookingStatusHistoryService = bookingStatusHistoryService;
            _paymentService = paymentService;
            _serviceRepository = serviceRepository;
            _bookingServiceRepository = bookingServiceRepository;
            _roomService = roomService;
            _bookingRepository = bookingRepository;
            _publishEndpoint = publishEndpoint;
        }

        public override async Task<BookingDto?> GetByIdAsync(int id)
        {
            var entity = await _bookingRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<BookingDto>(entity);
        }

        public override async Task<IEnumerable<BookingDto>> GetAllAsync()
        {
            var result = await _bookingRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BookingDto>>(result);
        }

        public override async Task<IEnumerable<BookingDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            var entities = await _bookingRepository.GetAllAsync();
            var skip = (pageNumber - 1) * pageSize;
            var page = entities.Skip(skip).Take(pageSize);
            return _mapper.Map<IEnumerable<BookingDto>>(page);
        }

        public async Task<IEnumerable<BookingDto>> GetPaidBookingsByUserIdAsync(int userId)
        {
            var bookings = await _repository.GetAllAsync();
            var userBookings = bookings.Where(b => b.UserId == userId && !b.IsDeleted);

            var paidBookings = new List<BookingDto>();
            foreach (var booking in userBookings)
            {
                var payments = await _paymentService.GetByBookingIdAsync(booking.Id);
                if (payments.Any(p => p.Status == PaymentStatus.Completed))
                {
                    paidBookings.Add(_mapper.Map<BookingDto>(booking));
                }
            }
            return paidBookings;
        }

        public async Task<IEnumerable<BookingDto>> GetNoPaidBookingsByUserIdAsync(int userId)
        {
            var bookings = await _repository.GetAllAsync();
            var userBookings = bookings.Where(b => b.UserId == userId && !b.IsDeleted);

            var paidBookings = new List<BookingDto>();
            foreach (var booking in userBookings)
            {
                var payments = await _paymentService.GetByBookingIdAsync(booking.Id);
                if (payments.Any(p => p.Status != PaymentStatus.Completed))
                {
                    paidBookings.Add(_mapper.Map<BookingDto>(booking));
                }
            }
            return paidBookings;
        }

        public async Task<IEnumerable<BookingDto>> GetByGuestIdAsync(int guestId)
        {
            try
            {
                _logger.LogInformation("Getting bookings for guest ID: {GuestId}", guestId);
                var entities = await _bookingRepository.GetAllAsync();
                var filteredEntities = entities.Where(b => b.UserId == guestId && !b.IsDeleted)
                                             .OrderByDescending(b => b.CreatedAt);
                return _mapper.Map<IEnumerable<BookingDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for guest ID: {GuestId}", guestId);
                throw;
            }
        }

        public async Task<IEnumerable<BookingDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting bookings for user ID: {UserId}", userId);
                var entities = await _bookingRepository.GetAllAsync();
                var filteredEntities = entities.Where(b => b.UserId == userId && !b.IsDeleted)
                                             .OrderByDescending(b => b.CreatedAt);
                return _mapper.Map<IEnumerable<BookingDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<BookingDto>> GetByRoomIdAsync(int roomId)
        {
            try
            {
                _logger.LogInformation("Getting bookings for room ID: {RoomId}", roomId);
                var entities = await _bookingRepository.GetAllAsync();
                var filteredEntities = entities.Where(b => b.RoomId == roomId && !b.IsDeleted)
                                             .OrderByDescending(b => b.CreatedAt);
                return _mapper.Map<IEnumerable<BookingDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for room ID: {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<BookingDto>> GetByStatusAsync(BookingStatus status)
        {
            try
            {
                _logger.LogInformation("Getting bookings with status: {Status}", status);
                var entities = await _bookingRepository.GetAllAsync();
                var filteredEntities = entities.Where(b => b.Status == status && !b.IsDeleted)
                                             .OrderByDescending(b => b.CreatedAt);
                return _mapper.Map<IEnumerable<BookingDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings with status: {Status}", status);
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(int id, int? cancelledByUserId = null)
        {
            try
            {
                _logger.LogInformation("Cancelling booking {BookingId}", id);

                var booking = await _repository.GetByIdAsync(id);
                if (booking == null || booking.IsDeleted)
                {
                    _logger.LogWarning("Booking {BookingId} not found for cancellation", id);
                    return false;
                }

                if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.CheckedOut)
                {
                    _logger.LogWarning("Booking {BookingId} cannot be cancelled - status is {Status}", id, booking.Status);
                    return false;
                }

                var oldStatus = booking.Status;
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(booking);

                // Log status change
                await LogBookingStatusChangeAsync(id, oldStatus, BookingStatus.Cancelled, "Booking cancelled", cancelledByUserId);

                _logger.LogInformation("Booking {BookingId} cancelled successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                throw;
            }
        }

        public async Task<bool> CheckInAsync(int id, int? checkedInByUserId = null)
        {
            try
            {
                _logger.LogInformation("Checking in booking {BookingId}", id);

                var booking = await _repository.GetByIdAsync(id);
                if (booking == null || booking.IsDeleted)
                {
                    _logger.LogWarning("Booking {BookingId} not found for check-in", id);
                    return false;
                }

                var oldStatus = booking.Status;
                booking.Status = BookingStatus.CheckedIn;
                booking.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(booking);

                // Log status change
                await LogBookingStatusChangeAsync(id, oldStatus, BookingStatus.CheckedIn, "Guest checked in", checkedInByUserId);

                await _publishEndpoint.Publish(new BookingUpdated(booking.Id, booking.Status.ToString(), booking.UserId, booking.RoomId));

                _logger.LogInformation("Booking {BookingId} checked in successfully", id);

                // Reminder scheduling privremeno isključen dok se ne omogući delayed scheduler
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in booking {BookingId}", id);
                throw;
            }
        }

        public async Task<bool> CheckOutAsync(int id, int? checkedOutByUserId = null)
        {
            try
            {
                _logger.LogInformation("Checking out booking {BookingId}", id);

                var booking = await _repository.GetByIdAsync(id);
                if (booking == null || booking.IsDeleted)
                {
                    _logger.LogWarning("Booking {BookingId} not found for check-out", id);
                    return false;
                }

                if (booking.Status != BookingStatus.CheckedIn)
                {
                    _logger.LogWarning("Booking {BookingId} cannot be checked out - status is {Status}", id, booking.Status);
                    return false;
                }

                var oldStatus = booking.Status;
                booking.Status = BookingStatus.CheckedOut;
                booking.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(booking);

                // Log status change
                await LogBookingStatusChangeAsync(id, oldStatus, BookingStatus.CheckedOut, "Guest checked out", checkedOutByUserId);

                await _publishEndpoint.Publish(new BookingUpdated(booking.Id, booking.Status.ToString(), booking.UserId, booking.RoomId));

                _logger.LogInformation("Booking {BookingId} checked out successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking out booking {BookingId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Getting bookings between {StartDate} and {EndDate}", startDate, endDate);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(b =>
                    !b.IsDeleted &&
                    b.CheckInDate < endDate &&
                    b.CheckOutDate > startDate)
                    .OrderBy(b => b.CheckInDate);
                return _mapper.Map<IEnumerable<BookingDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null)
        {
            try
            {
                _logger.LogInformation("Checking room {RoomId} availability from {CheckIn} to {CheckOut}", roomId, checkIn, checkOut);

                var entities = await _repository.GetAllAsync();
                var conflictingBookings = entities.Where(b =>
                    !b.IsDeleted &&
                    b.RoomId == roomId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut &&
                    (excludeBookingId == null || b.Id != excludeBookingId) &&
                    b.CheckInDate < checkOut &&
                    b.CheckOutDate > checkIn);

                var isAvailable = !conflictingBookings.Any();
                _logger.LogInformation("Room {RoomId} availability: {IsAvailable}", roomId, isAvailable);

                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room {RoomId} availability", roomId);
                throw;
            }
        }

        public override async Task<BookingDto> CreateAsync(CreateBookingDto createDto)
        {
            try
            {
                var isAvailable = await IsRoomAvailableAsync(createDto.RoomId, createDto.CheckInDate, createDto.CheckOutDate);
                if (!isAvailable)
                    throw new InvalidOperationException("Room is not available for the selected dates");

                var room = await _roomService.GetByIdAsync(createDto.RoomId) ?? throw new InvalidOperationException("Room not found.");

                // Base price (mirror logic from RoomService)
                var nights = (createDto.CheckOutDate.Date - createDto.CheckInDate.Date).Days;
                if (nights <= 0) throw new ArgumentException("Stay must be at least one night.");
                decimal total = nights * room.PricePerNight;

                var booking = new Booking
                {
                    CheckInDate = createDto.CheckInDate,
                    CheckOutDate = createDto.CheckOutDate,
                    NumberOfGuests = createDto.NumberOfGuests,
                    SpecialRequests = createDto.SpecialRequests ?? string.Empty,
                    RoomId = createDto.RoomId,
                    UserId = createDto.UserId,
                    Status = BookingStatus.Pending,
                };

                // Attach services (optional)
                var serviceItems = new List<API.Models.BookingService>();
                if (createDto.Services != null)
                {
                    foreach (var item in createDto.Services)
                    {
                        var svc = await _serviceRepository.GetByIdAsync(item.ServiceId);
                        if (svc == null || !svc.IsAvailable) continue; // ili baci exception
                        if (svc.HotelId != room.HotelId)
                            throw new InvalidOperationException("Selected service does not belong to the room's hotel.");
                        var qty = item.Quantity <= 0 ? 1 : item.Quantity;
                        serviceItems.Add(new API.Models.BookingService
                        {
                            ServiceId = svc.Id,
                            UnitPrice = svc.Price,
                            Quantity = qty
                        });
                        total += svc.Price * qty;
                    }
                }

                booking.TotalPrice = total;

                // Persist booking
                await _repository.AddAsync(booking);

                // Persist booking services
                if (serviceItems.Count > 0)
                {
                    foreach (var bs in serviceItems)
                    {
                        bs.BookingId = booking.Id;
                        await _bookingServiceRepository.AddAsync(bs);
                    }
                }

                // Log initial status
                await LogBookingStatusChangeAsync(booking.Id, BookingStatus.Pending, BookingStatus.Pending, "Booking created", createDto.UserId);

                // Publish BookingCreated event
                await _publishEndpoint.Publish(new BookingCreated(
                    booking.Id,
                    booking.UserId,
                    booking.RoomId,
                    room.HotelId,
                    booking.CheckInDate,
                    booking.CheckOutDate
                ));

                return _mapper.Map<BookingDto>(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                throw;
            }
        }

        private async Task LogBookingStatusChangeAsync(int bookingId, BookingStatus fromStatus, BookingStatus toStatus, string? notes, int? changedByUserId)
        {
            try
            {
                var statusHistoryDto = new CreateBookingStatusHistoryDto
                {
                    BookingId = bookingId,
                    FromStatus = fromStatus,
                    ToStatus = toStatus,
                    Notes = notes,
                    ChangedByUserId = changedByUserId
                };

                await _bookingStatusHistoryService.CreateAsync(statusHistoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging booking status change for booking {BookingId}", bookingId);
            }
        }
    }
}
