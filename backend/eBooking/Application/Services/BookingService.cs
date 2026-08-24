using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class BookingService : BaseDtoService<Booking, BookingDto, CreateBookingDto, UpdateBookingDto>, IBookingService
    {
        private readonly IBookingStatusHistoryService _bookingStatusHistoryService;
        private readonly IPaymentService _paymentService;
        private readonly IRepository<Service> _serviceRepository;
        private readonly IRepository<Persistence.Models.BookingService> _bookingServiceRepository;
        private readonly IRoomService _roomService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        // Gornja granica za list endpointe koji nemaju eksplicitnu paginaciju u ugovoru API-ja
        // (GetByUserId/ByGuestId/ByRoomId/ByStatus/ByDateRange) — uputa tretira endpointe bez
        // definisanog maksimalnog limita kao grešku za neprihvatanje.
        private const int MaxUnboundedResults = 200;

        public BookingService(
            IRepository<Booking> repository,
            IMapper mapper,
            ILogger<BookingService> logger,
            IBookingStatusHistoryService bookingStatusHistoryService,
            IPaymentService paymentService,
            IRepository<Service> serviceRepository,
            IRepository<Persistence.Models.BookingService> bookingServiceRepository,
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
            var bookings = await _bookingRepository.GetAllAsync();
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
            var bookings = await _bookingRepository.GetAllAsync();
            var userBookings = bookings.Where(b => b.UserId == userId && !b.IsDeleted);

            var paidBookings = new List<BookingDto>();
            foreach (var booking in userBookings)
            {
                var payments = await _paymentService.GetByBookingIdAsync(booking.Id);
                // "Neplaćena" = NEMA Completed plaćanje — obuhvata i rezervacije bez ijednog
                // plaćanja (npr. kreirane direktno na recepciji preko desktop app-a, bez online
                // checkout-a). Prijašnji uslov (payments.Any(p => p.Status != Completed)) je
                // zahtijevao da POSTOJI barem jedno ne-Completed plaćanje, pa je rezervacija bez
                // ijednog plaćanja nestajala i iz ove i iz "paid" liste — gost je nikad ne bi vidio
                // na mobile app-u.
                if (!payments.Any(p => p.Status == PaymentStatus.Completed))
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
                                             .OrderByDescending(b => b.CreatedAt)
                                             .Take(MaxUnboundedResults);
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
                                             .OrderByDescending(b => b.CreatedAt)
                                             .Take(MaxUnboundedResults);
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
                                             .OrderByDescending(b => b.CreatedAt)
                                             .Take(MaxUnboundedResults);
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
                                             .OrderByDescending(b => b.CreatedAt)
                                             .Take(MaxUnboundedResults);
                return _mapper.Map<IEnumerable<BookingDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings with status: {Status}", status);
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(int id, int? cancelledByUserId = null, string? reason = null)
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

                // Log status change (razlog otkazivanja ide u Notes istorijskog zapisa)
                var notes = string.IsNullOrWhiteSpace(reason) ? "Booking cancelled" : $"Booking cancelled: {reason}";
                await LogBookingStatusChangeAsync(id, oldStatus, BookingStatus.Cancelled, notes, cancelledByUserId);

                // Automatski refund plaćanja pri otkazivanju — RefundPaymentAsync je već potpuno
                // implementiran (zove Stripe provider i ažurira status plaćanja), samo se do sada
                // nigdje nije pozivao pri otkazivanju rezervacije. Bez ovoga plaćanje ostaje
                // "Completed" i rezervacija se pogrešno i dalje prikazuje kao plaćena/aktivna
                // (npr. na mobile app-u) iako je rezervacija otkazana.
                try
                {
                    var payments = await _paymentService.GetByBookingIdAsync(booking.Id);
                    var refundablePayment = payments.FirstOrDefault(p =>
                        p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.PartiallyRefunded);
                    if (refundablePayment != null)
                    {
                        var alreadyRefunded = refundablePayment.RefundAmount ?? 0;
                        var remaining = refundablePayment.Amount - alreadyRefunded;
                        if (remaining > 0)
                        {
                            var refunded = await _paymentService.RefundPaymentAsync(
                                refundablePayment.Id, remaining, "Booking cancelled", cancelledByUserId);
                            if (!refunded)
                            {
                                _logger.LogWarning(
                                    "Refund nije uspio za booking {BookingId}, payment {PaymentId}",
                                    id, refundablePayment.Id);
                            }
                        }
                    }
                }
                catch (Exception refundEx)
                {
                    // Ne smije srušiti otkazivanje ako refund ne uspije (npr. Stripe nedostupan) —
                    // rezervacija ostaje otkazana, refund se po potrebi može ručno ponoviti kasnije.
                    _logger.LogError(refundEx, "Greška pri automatskom refundu za booking {BookingId}", id);
                }

                // Notifikacija korisniku o otkazivanju — bez ovoga korisnik ne saznaje da je
                // rezervacija otkazana (npr. kad admin otkaže rezervaciju).
                await _publishEndpoint.Publish(new BookingUpdated(booking.Id, booking.Status.ToString(), booking.UserId, booking.RoomId));

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

                // Samo potvrđena rezervacija smije preći u CheckedIn — bez ovoga bi bilo moguće
                // "prijaviti" gosta na Pending ili već otkazanu rezervaciju.
                if (booking.Status != BookingStatus.Confirmed)
                {
                    _logger.LogWarning("Booking {BookingId} cannot be checked in - status is {Status}", id, booking.Status);
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

        public async Task<bool> MarkNoShowAsync(int id, int? markedByUserId = null)
        {
            try
            {
                _logger.LogInformation("Marking booking {BookingId} as no-show", id);

                var booking = await _repository.GetByIdAsync(id);
                if (booking == null || booking.IsDeleted)
                {
                    _logger.LogWarning("Booking {BookingId} not found for no-show", id);
                    return false;
                }

                // Gost se može proglasiti "no-show" samo ako je rezervacija bila potvrđena (ili je
                // ostala Pending) i nikad nije stvarno stigao (nije CheckedIn/CheckedOut/otkazana).
                if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
                {
                    _logger.LogWarning("Booking {BookingId} cannot be marked no-show - status is {Status}", id, booking.Status);
                    return false;
                }

                var oldStatus = booking.Status;
                booking.Status = BookingStatus.NoShow;
                booking.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(booking);

                await LogBookingStatusChangeAsync(id, oldStatus, BookingStatus.NoShow, "Guest marked as no-show", markedByUserId);

                await _publishEndpoint.Publish(new BookingUpdated(booking.Id, booking.Status.ToString(), booking.UserId, booking.RoomId));

                _logger.LogInformation("Booking {BookingId} marked as no-show successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking booking {BookingId} as no-show", id);
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
                    .OrderBy(b => b.CheckInDate)
                    .Take(MaxUnboundedResults);
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
                if (createDto.NumberOfGuests <= 0)
                    throw new ArgumentException("Broj gostiju mora biti najmanje 1.");

                var room = await _roomService.GetByIdAsync(createDto.RoomId) ?? throw new InvalidOperationException("Soba nije pronađena.");

                if (createDto.CheckInDate >= createDto.CheckOutDate)
                    throw new ArgumentException("Datum prijave mora biti prije datuma odjave.");

                // Provjera preklapanja termina na backendu — ne smije se oslanjati samo na
                // frontend provjeru dostupnosti, jer dva zahtjeva mogu stići istovremeno.
                var isAvailable = await IsRoomAvailableAsync(createDto.RoomId, createDto.CheckInDate, createDto.CheckOutDate);
                if (!isAvailable)
                    throw new InvalidOperationException("Odabrana soba nije dostupna za izabrani period.");

                var serviceSelections = createDto.Services?
                    .Select(s => (s.ServiceId, s.Quantity))
                    .ToList() ?? new List<(int ServiceId, int Quantity)>();

                var total = await _roomService.CalculatePriceAsync(
                    createDto.RoomId,
                    createDto.CheckInDate,
                    createDto.CheckOutDate,
                    createDto.NumberOfGuests,
                    serviceSelections);

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

                // Attach services (optional) — total već uključuje servise iz CalculatePriceAsync
                var serviceItems = new List<Persistence.Models.BookingService>();
                if (createDto.Services != null)
                {
                    foreach (var item in createDto.Services)
                    {
                        var svc = await _serviceRepository.GetByIdAsync(item.ServiceId);
                        if (svc == null || !svc.IsAvailable) continue;
                        if (svc.HotelId != room.HotelId)
                            throw new InvalidOperationException("Odabrana usluga ne pripada hotelu ove sobe.");
                        var qty = item.Quantity <= 0 ? 1 : item.Quantity;
                        serviceItems.Add(new Persistence.Models.BookingService
                        {
                            ServiceId = svc.Id,
                            UnitPrice = svc.Price,
                            Quantity = qty
                        });
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

        /// <summary>
        /// Confirm a booking after successful payment
        /// </summary>
        /// <param name="bookingId">Booking ID to confirm</param>
        /// <param name="paymentId">Payment ID that triggered the confirmation</param>
        /// <returns>True if booking was successfully confirmed</returns>
        public async Task<bool> ConfirmBookingAfterPaymentAsync(int bookingId, int paymentId)
        {
            try
            {
                _logger.LogInformation("Confirming booking {BookingId} after payment {PaymentId}", bookingId, paymentId);

                var booking = await _repository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking {BookingId} not found for confirmation", bookingId);
                    return false;
                }

                if (booking.Status != BookingStatus.Pending)
                {
                    _logger.LogWarning("Booking {BookingId} cannot be confirmed - current status is {Status}", bookingId, booking.Status);
                    return false;
                }

                // Update booking status to Confirmed
                var previousStatus = booking.Status;
                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(booking);

                // Log the status change
                await LogBookingStatusChangeAsync(bookingId, previousStatus, BookingStatus.Confirmed, 
                    $"Booking confirmed after successful payment {paymentId}", null);

                // Publish BookingConfirmed event
                await _publishEndpoint.Publish(new BookingConfirmed(bookingId, booking.UserId, paymentId));

                _logger.LogInformation("Booking {BookingId} successfully confirmed after payment {PaymentId}", bookingId, paymentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId} after payment {PaymentId}", bookingId, paymentId);
                return false;
            }
        }

        public async Task<BookingStatistics> GetBookingStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var query = entities.Where(b => !b.IsDeleted);

                if (fromDate.HasValue)
                    query = query.Where(b => b.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(b => b.CreatedAt <= toDate.Value);

                var totalBookings = query.Count();
                var confirmedBookings = query.Count(b => b.Status == BookingStatus.Confirmed);
                var cancelledBookings = query.Count(b => b.Status == BookingStatus.Cancelled);
                var pendingBookings = query.Count(b => b.Status == BookingStatus.Pending);
                var completedBookings = query.Count(b => b.Status == BookingStatus.CheckedOut);
                var totalRevenue = query.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedOut)
                    .Sum(b => b.TotalPrice);
                var averageBookingValue = totalBookings > 0 ? (double)(totalRevenue / totalBookings) : 0;

                // Calculate occupancy rate (simplified - would need room data for accurate calculation)
                var averageOccupancyRate = 0.0; // TODO: Implement proper occupancy calculation

                // Monthly data for last 12 months
                var monthlyData = new List<MonthlyBookingData>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.UtcNow.AddMonths(-i).Date.AddDays(1 - DateTime.UtcNow.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    
                    var monthQuery = query.Where(b => b.CreatedAt >= monthStart && b.CreatedAt <= monthEnd);
                    var monthBookings = monthQuery.Count();
                    var monthRevenue = monthQuery.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedOut)
                        .Sum(b => b.TotalPrice);

                    monthlyData.Add(new MonthlyBookingData
                    {
                        Month = monthStart.ToString("MMM yyyy"),
                        BookingCount = monthBookings,
                        TotalRevenue = monthRevenue,
                        OccupancyRate = 0.0 // TODO: Calculate actual occupancy
                    });
                }

                return new BookingStatistics
                {
                    TotalBookings = totalBookings,
                    ConfirmedBookings = confirmedBookings,
                    CancelledBookings = cancelledBookings,
                    PendingBookings = pendingBookings,
                    CompletedBookings = completedBookings,
                    TotalRevenue = totalRevenue,
                    AverageBookingValue = averageBookingValue,
                    AverageOccupancyRate = averageOccupancyRate,
                    FromDate = fromDate,
                    ToDate = toDate,
                    MonthlyData = monthlyData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating booking statistics");
                throw;
            }
        }
    }
}
