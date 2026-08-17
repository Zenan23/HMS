using API.Attributes;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : BaseController<BookingDto, CreateBookingDto, UpdateBookingDto>
    {
        private readonly IBookingService _bookingService;

        public BookingsController(
            IBookingService bookingService,
            ILogger<BookingsController> logger)
            : base(bookingService, logger)
        {
            _bookingService = bookingService;
        }

        // Lista SVIH rezervacija (bez filtera po korisniku) — samo za osoblje, inače bi bilo koji
        // ulogovani gost mogao vidjeti tuđe rezervacije.
        [HttpGet]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<PaginatedResult<BookingDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => base.GetAll(pageNumber, pageSize);

        // Kreiranje rezervacije: UserId se PRESKRIPUJE identitetom iz JWT-a za obične goste, jer
        // createDto.UserId dolazi iz klijentovog body-ja i ne smije se vjerovati (gost ne smije
        // moći kreirati rezervaciju "u ime" drugog korisnika). Osoblje (Employee/Admin) i dalje
        // smije kreirati rezervaciju za bilo kojeg korisnika (npr. rezervacija na recepciji).
        [HttpPost]
        public override async Task<ActionResult<ApiResponse<BookingDto>>> Create([FromBody] CreateBookingDto createDto)
        {
            if (!IsElevated())
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Forbid();
                }
                createDto.UserId = currentUserId.Value;
            }

            return await base.Create(createDto);
        }

        // Pojedinačna rezervacija po ID-u — spriječi da gost pogodi/enumeriše tuđi bookingId
        // i vidi detalje (datumi, cijena) tuđe rezervacije.
        [HttpGet("{id}")]
        public override async Task<ActionResult<ApiResponse<BookingDto>>> GetById([FromRoute] int id)
        {
            var result = await base.GetById(id);
            if (result.Result is OkObjectResult ok && ok.Value is ApiResponse<BookingDto> resp && resp.Data != null)
            {
                if (!IsSelfOrElevated(resp.Data.UserId))
                {
                    return Forbid();
                }
            }
            return result;
        }

        [HttpGet("user/{userId}/paid")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetPaidBookingsByUserId([FromRoute] int userId)
        {
            if (!IsSelfOrElevated(userId))
            {
                return Forbid();
            }

            var bookings = await _bookingService.GetPaidBookingsByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResult(bookings, "Paid bookings retrieved successfully."));
        }

        [HttpGet("user/{userId}/nopaid")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetNoPaidBookingsByUserId([FromRoute] int userId)
        {
            if (!IsSelfOrElevated(userId))
            {
                return Forbid();
            }

            var bookings = await _bookingService.GetNoPaidBookingsByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResult(bookings, "Paid bookings retrieved successfully."));
        }

        /// <summary>
        /// Get bookings by guest ID
        /// </summary>
        /// <param name="guestId">Guest ID</param>
        /// <returns>List of bookings</returns>
        [HttpGet("guest/{guestId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetNByGuestId([FromRoute] int guestId)
        {
            try
            {
                if (guestId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<BookingDto>>.ErrorResult("Invalid guest ID."));
                }

                if (!IsSelfOrElevated(guestId))
                {
                    return Forbid();
                }

                var bookings = await _bookingService.GetByGuestIdAsync(guestId);
                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResult(bookings, "Bookings retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for guest ID: {GuestId}", guestId);
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResult("An error occurred while retrieving bookings."));
            }
        }

        /// <summary>
        /// Get bookings by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of bookings</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetByUserId([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<BookingDto>>.ErrorResult("Invalid user ID."));
                }

                if (!IsSelfOrElevated(userId))
                {
                    return Forbid();
                }

                var bookings = await _bookingService.GetByUserIdAsync(userId);
                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResult(bookings, "Bookings retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResult("An error occurred while retrieving bookings."));
            }
        }

        /// <summary>
        /// Get bookings by room ID
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <returns>List of bookings</returns>
        [HttpGet("room/{roomId}")]
        [AuthorizeRole(UserRole.Employee)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetByRoomId([FromRoute] int roomId)
        {
            try
            {
                if (roomId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<BookingDto>>.ErrorResult("Invalid room ID."));
                }

                var bookings = await _bookingService.GetByRoomIdAsync(roomId);
                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResult(bookings, "Bookings retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for room ID: {RoomId}", roomId);
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResult("An error occurred while retrieving bookings."));
            }
        }

        /// <summary>
        /// Get bookings by status
        /// </summary>
        /// <param name="status">Booking status</param>
        /// <returns>List of bookings</returns>
        [HttpGet("status/{status}")]
        [AuthorizeRole(UserRole.Employee)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetByStatus([FromRoute] BookingStatus status)
        {
            try
            {
                var bookings = await _bookingService.GetByStatusAsync(status);
                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResult(bookings, "Bookings retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings with status: {Status}", status);
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResult("An error occurred while retrieving bookings."));
            }
        }

        /// <summary>
        /// Get bookings by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of bookings</returns>
        [HttpGet("date-range")]
        [AuthorizeRole(UserRole.Employee)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetByDateRange(
            [FromQuery][Required] DateTime startDate,
            [FromQuery][Required] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return BadRequest(ApiResponse<IEnumerable<BookingDto>>.ErrorResult("Start date must be before end date."));
                }

                var bookings = await _bookingService.GetBookingsByDateRangeAsync(startDate, endDate);
                return Ok(ApiResponse<IEnumerable<BookingDto>>.SuccessResult(bookings, "Bookings retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for date range {StartDate} to {EndDate}", startDate, endDate);
                return StatusCode(500, ApiResponse<IEnumerable<BookingDto>>.ErrorResult("An error occurred while retrieving bookings."));
            }
        }

        /// <summary>
        /// Check room availability
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="checkIn">Check-in date</param>
        /// <param name="checkOut">Check-out date</param>
        /// <param name="excludeBookingId">Booking ID to exclude (optional)</param>
        /// <returns>Availability status</returns>
        [HttpGet("room/{roomId}/availability")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckRoomAvailability(
            [FromRoute] int roomId,
            [FromQuery][Required] DateTime checkIn,
            [FromQuery][Required] DateTime checkOut,
            [FromQuery] int? excludeBookingId = null)
        {
            try
            {
                if (roomId <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid room ID."));
                }

                if (checkIn >= checkOut)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Check-in date must be before check-out date."));
                }

                var isAvailable = await _bookingService.IsRoomAvailableAsync(roomId, checkIn, checkOut, excludeBookingId);
                return Ok(ApiResponse<bool>.SuccessResult(isAvailable, $"Room availability checked. Available: {isAvailable}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room {RoomId} availability", roomId);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while checking room availability."));
            }
        }

        // Sve statusne prelaze rezervacije moraju ići kroz eksplicitne, centralizovane servisne
        // metode (CancelBookingAsync/CheckInAsync/CheckOutAsync/MarkNoShowAsync) — NIKAD kroz
        // generički BaseDtoService.UpdateAsync, koji bi bez validacije upisao klijentov Status i
        // TotalPrice direktno u bazu. Takođe se provjerava vlasništvo prije bilo koje izmjene:
        // Guest smije samo otkazati SVOJU rezervaciju, ostale statusne prelaze smije izvesti
        // isključivo osoblje (Employee/Admin).
        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Guest)]
        public override async Task<ActionResult<ApiResponse<BookingDto>>> Update([FromRoute] int id, [FromBody] UpdateBookingDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid booking ID."));
                }

                var existing = await _bookingService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"Booking with ID {id} not found."));
                }

                if (!IsSelfOrElevated(existing.UserId))
                {
                    return Forbid();
                }

                var currentUserId = GetCurrentUserId();
                bool result;
                switch (updateDto.Status)
                {
                    case BookingStatus.Cancelled:
                        result = await _bookingService.CancelBookingAsync(id, currentUserId);
                        break;
                    case BookingStatus.CheckedIn:
                        if (!IsElevated()) return Forbid();
                        result = await _bookingService.CheckInAsync(id, currentUserId);
                        break;
                    case BookingStatus.CheckedOut:
                        if (!IsElevated()) return Forbid();
                        result = await _bookingService.CheckOutAsync(id, currentUserId);
                        break;
                    case BookingStatus.NoShow:
                        if (!IsElevated()) return Forbid();
                        result = await _bookingService.MarkNoShowAsync(id, currentUserId);
                        break;
                    default:
                        // Pending/Confirmed se ne postavljaju direktno kroz PUT — Confirmed nastaje
                        // isključivo kroz ConfirmBookingAfterPaymentAsync (nakon uspješnog plaćanja),
                        // a Pending je isključivo početno stanje pri kreiranju rezervacije. Ovdje
                        // NEMA fallback-a na base.Update, jer bi to dozvolilo klijentu da proizvoljno
                        // upiše Status i TotalPrice bez validacije.
                        return BadRequest(ApiResponse<bool>.ErrorResult(
                            "Status rezervacije se ne može direktno postaviti na ovu vrijednost. " +
                            "Koristite akcije za otkazivanje, check-in, check-out ili no-show."));
                }

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Operation cannot be completed. Check booking status."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(true, $"Booking operation executed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for ID: {BookingId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while updating the booking status."));
            }
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Cancellation request</param>
        /// <returns>Cancellation result</returns>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelBooking([FromRoute] int id, [FromBody] CancelBookingRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid booking ID."));
                }

                var existing = await _bookingService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"Booking with ID {id} not found."));
                }

                // Vlasništvo: gost smije otkazati samo svoju rezervaciju; userId se uzima iz JWT-a,
                // nikad iz body-ja (request.CancelledByUserId je uklonjen iz DTO-a).
                if (!IsSelfOrElevated(existing.UserId))
                {
                    return Forbid();
                }

                var result = await _bookingService.CancelBookingAsync(id, GetCurrentUserId(), request.Reason);

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Booking cannot be cancelled. Check booking status."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Booking cancelled successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking with ID: {BookingId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while cancelling the booking."));
            }
        }

        /// <summary>
        /// Check in a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Check-in request</param>
        /// <returns>Check-in result</returns>
        [HttpPost("{id}/checkin")]
        [AuthorizeRole(UserRole.Employee)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckIn([FromRoute] int id, [FromBody] CheckInRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid booking ID."));
                }

                var result = await _bookingService.CheckInAsync(id, GetCurrentUserId());

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Booking cannot be checked in. Check booking status."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Booking checked in successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in booking with ID: {BookingId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while checking in the booking."));
            }
        }

        /// <summary>
        /// Check out a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Check-out request</param>
        /// <returns>Check-out result</returns>
        [HttpPost("{id}/checkout")]
        [AuthorizeRole(UserRole.Employee)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckOut([FromRoute] int id, [FromBody] CheckOutRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid booking ID."));
                }

                var result = await _bookingService.CheckOutAsync(id, GetCurrentUserId());

                if (!result)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Booking cannot be checked out. Check booking status."));
                }

                return Ok(ApiResponse<bool>.SuccessResult(result, "Booking checked out successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking out booking with ID: {BookingId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while checking out the booking."));
            }
        }
    }

    public class CancelBookingRequest
    {
        /// <summary>Razlog otkazivanja koji korisnik/osoblje unosi — prikazuje se u historiji rezervacije.</summary>
        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? Reason { get; set; }
    }

    public class CheckInRequest
    {
        public int? CheckedInByUserId { get; set; }
    }

    public class CheckOutRequest
    {
        public int? CheckedOutByUserId { get; set; }
    }

}
