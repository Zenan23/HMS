using API.Attributes;
using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : BaseController<RoomDto, CreateRoomDto, UpdateRoomDto>
    {
        private readonly IRoomService _roomService;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;

        public RoomsController(
            IRoomService roomService,
            IPaymentService paymentService,
            IMapper mapper,
            ILogger<RoomsController> logger)
            : base(roomService, logger)
        {
            _roomService = roomService;
            _paymentService = paymentService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get rooms by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>Rooms in the specified hotel</returns>
        [HttpGet("by-hotel/{hotelId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoomDto>>>> GetRoomsByHotel([FromRoute] int hotelId)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByHotelIdAsync(hotelId);
                var roomDtos = _mapper.Map<IEnumerable<RoomDto>>(rooms);

                return Ok(ApiResponse<IEnumerable<RoomDto>>.SuccessResult(
                    roomDtos,
                    $"Rooms for hotel {hotelId} retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms for hotel: {HotelId}", hotelId);
                return StatusCode(500, ApiResponse<IEnumerable<RoomDto>>.ErrorResult("An error occurred while retrieving rooms."));
            }
        }

        /// <summary>
        /// Get available rooms for specific dates
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <param name="checkIn">Check-in date</param>
        /// <param name="checkOut">Check-out date</param>
        /// <returns>Available rooms</returns>
        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoomDto>>>> GetAvailableRooms(
            [FromQuery] int hotelId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            try
            {
                if (checkIn >= checkOut)
                {
                    return BadRequest(ApiResponse<IEnumerable<RoomDto>>.ErrorResult("Check-in date must be before check-out date."));
                }

                if (checkIn < DateTime.Today)
                {
                    return BadRequest(ApiResponse<IEnumerable<RoomDto>>.ErrorResult("Check-in date cannot be in the past."));
                }

                var rooms = await _roomService.GetAvailableRoomsAsync(hotelId, checkIn, checkOut);
                var roomDtos = _mapper.Map<IEnumerable<RoomDto>>(rooms);

                return Ok(ApiResponse<IEnumerable<RoomDto>>.SuccessResult(
                    roomDtos,
                    $"Available rooms retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available rooms for hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse<IEnumerable<RoomDto>>.ErrorResult("An error occurred while retrieving available rooms."));
            }
        }

        /// <summary>
        /// Get rooms by type
        /// </summary>
        /// <param name="roomType">Room type</param>
        /// <returns>Rooms of the specified type</returns>
        [HttpGet("by-type/{roomType}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoomDto>>>> GetRoomsByType([FromRoute] RoomType roomType)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByTypeAsync(roomType);
                var roomDtos = _mapper.Map<IEnumerable<RoomDto>>(rooms);

                return Ok(ApiResponse<IEnumerable<RoomDto>>.SuccessResult(
                    roomDtos,
                    $"Rooms of type {roomType} retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms by type: {RoomType}", roomType);
                return StatusCode(500, ApiResponse<IEnumerable<RoomDto>>.ErrorResult("An error occurred while retrieving rooms."));
            }
        }

        /// <summary>
        /// Get rooms by price range
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <returns>Rooms within the price range</returns>
        [HttpGet("by-price")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoomDto>>>> GetRoomsByPriceRange(
            [FromQuery] decimal minPrice,
            [FromQuery] decimal maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                {
                    return BadRequest(ApiResponse<IEnumerable<RoomDto>>.ErrorResult("Invalid price range."));
                }

                var rooms = await _roomService.GetRoomsByPriceRangeAsync(minPrice, maxPrice);
                var roomDtos = _mapper.Map<IEnumerable<RoomDto>>(rooms);

                return Ok(ApiResponse<IEnumerable<RoomDto>>.SuccessResult(
                    roomDtos,
                    $"Rooms in price range ${minPrice}-${maxPrice} retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms by price range: {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return StatusCode(500, ApiResponse<IEnumerable<RoomDto>>.ErrorResult("An error occurred while retrieving rooms."));
            }
        }

        /// <summary>
        /// Check room availability
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="checkIn">Check-in date</param>
        /// <param name="checkOut">Check-out date</param>
        /// <returns>Availability status</returns>
        [HttpGet("{roomId}/availability")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckRoomAvailability(
            [FromRoute] int roomId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut,
            [FromQuery] string? services = null)
        {
            try
            {
                if (checkIn >= checkOut)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Check-in date must be before check-out date."));
                }

                var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, checkIn, checkOut);
                return Ok(ApiResponse<bool>.SuccessResult(isAvailable, "Room availability checked successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room availability for room {RoomId}", roomId);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while checking room availability."));
            }
        }

        /// <summary>
        /// Calculate total price for a room based on dates and number of guests
        /// </summary>
        /// <param name="roomId">Room ID</param>
        /// <param name="checkIn">Check-in date</param>
        /// <param name="checkOut">Check-out date</param>
        /// <param name="guests">Number of guests</param>
        /// <returns>Total price for the stay</returns>
        [HttpGet("{roomId}/calculate-price")]
        public async Task<ActionResult<ApiResponse<decimal>>> CalculatePrice(
            [FromRoute] int roomId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut,
            [FromQuery] int guests = 1,
            [FromQuery] string? services = null)
        {
            if (checkIn >= checkOut)
                return BadRequest(ApiResponse<decimal>.ErrorResult("Check-in date must be before check-out date."));
            if (guests <= 0)
                return BadRequest(ApiResponse<decimal>.ErrorResult("Number of guests must be at least 1."));

            try
            {
                if (!string.IsNullOrWhiteSpace(services))
                {
                    var parsed = ParseServicesQuery(services);
                    var price1 = await _roomService.CalculatePriceAsync(roomId, checkIn, checkOut, guests, parsed);
                    return Ok(ApiResponse<decimal>.SuccessResult(price1, "Total price calculated successfully (with services)."));
                }

                var price = await _roomService.CalculatePriceAsync(roomId, checkIn, checkOut, guests);
                return Ok(ApiResponse<decimal>.SuccessResult(price, "Total price calculated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for room {RoomId}", roomId);
                return StatusCode(500, ApiResponse<decimal>.ErrorResult("An error occurred while calculating price."));
            }
        }

        private static IEnumerable<(int ServiceId, int Quantity)> ParseServicesQuery(string services)
        {
            // Format: "1:2,5:1,7" → default qty = 1
            var result = new List<(int ServiceId, int Quantity)>();
            var parts = services.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                var sub = p.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (sub.Length == 1)
                {
                    if (int.TryParse(sub[0], out var id)) result.Add((id, 1));
                }
                else if (sub.Length == 2)
                {
                    if (int.TryParse(sub[0], out var id) && int.TryParse(sub[1], out var qty))
                    {
                        result.Add((id, Math.Max(1, qty)));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Create a new room (Staff/Manager/Admin only)
        /// </summary>
        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override async Task<ActionResult<ApiResponse<RoomDto>>> Create([FromBody] CreateRoomDto createDto)
        {
            return await base.Create(createDto);
        }

        /// <summary>
        /// Update an existing room (Staff/Manager/Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override async Task<ActionResult<ApiResponse<RoomDto>>> Update([FromRoute] int id, [FromBody] UpdateRoomDto updateDto)
        {
            return await base.Update(id, updateDto);
        }

        /// <summary>
        /// Delete a room (Manager/Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
        {
            return await base.Delete(id);
        }
    }
}
