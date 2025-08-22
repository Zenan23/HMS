using AutoMapper;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class HotelService : BaseDtoService<Hotel,HotelDto,CreateHotelDto,UpdateHotelDto>, IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IReviewService _reviewService;
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<Booking> _bookingRepository;

        public HotelService(
            IHotelRepository hotelRepository,
            IReviewService reviewService,
            IRoomService roomService,
            IBookingService bookingService,
            IRepository<Room> roomRepository,
            IRepository<Booking> bookingRepository,
            IMapper mapper,
            ILogger<HotelService> logger)
            : base(hotelRepository, mapper, logger)
        {
            _hotelRepository = hotelRepository;
            _reviewService = reviewService;
            _roomService = roomService;
            _bookingService = bookingService;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync(int? rating = null, string city = null, string name = null)
        {
            // Dohvat svih hotela
            var hotels = await _hotelRepository.GetAllAsync();

            // Filtriranje hotela prema opcionalnim parametrima
            if (rating.HasValue)
            {
                hotels = hotels.Where(h => h.StarRating >= rating.Value);  // Filtriraj prema ocjeni
            }

            if (!string.IsNullOrEmpty(city))
            {
                hotels = hotels.Where(h => h.City.Contains(city, StringComparison.OrdinalIgnoreCase));  // Filtriraj prema gradu
            }

            if (!string.IsNullOrEmpty(name))
            {
                hotels = hotels.Where(h => h.Name.Contains(name, StringComparison.OrdinalIgnoreCase));  // Filtriraj prema imenu hotela
            }

            // Mapiranje hotela u DTO
            var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels).ToList();

            // Dohvat svih recenzija i grupisanje po hotelima
            var reviews = await _reviewService.GetAllAsync();
            var grouped = reviews
                .Where(r => !r.IsDeleted && r.IsApproved)
                .GroupBy(r => r.HotelId)
                .ToDictionary(g => g.Key, g => new { Avg = g.Average(x => (double)x.Rating), Cnt = g.Count() });

            // Dodavanje prosječne ocjene i broja recenzija za svaki hotel
            foreach (var dto in hotelDtos)
            {
                if (grouped.TryGetValue(dto.Id, out var agg))
                {
                    dto.AverageRating = Math.Round(agg.Avg, 2);
                    dto.ReviewsCount = agg.Cnt;
                }
                else
                {
                    dto.AverageRating = 0;
                    dto.ReviewsCount = 0;
                }
            }

            return hotelDtos;
        }

        public override async Task<IEnumerable<HotelDto>> GetAllAsync()
        {
            var hotels = await _hotelRepository.GetAllAsync();
            var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels).ToList();

            var reviews = await _reviewService.GetAllAsync();
            var grouped = reviews
                .Where(r => !r.IsDeleted && r.IsApproved)
                .GroupBy(r => r.HotelId)
                .ToDictionary(g => g.Key, g => new { Avg = g.Average(x => (double)x.Rating), Cnt = g.Count() });

            foreach (var dto in hotelDtos)
            {
                if (grouped.TryGetValue(dto.Id, out var agg))
                {
                    dto.AverageRating = Math.Round(agg.Avg, 2);
                    dto.ReviewsCount = agg.Cnt;
                }
                else
                {
                    dto.AverageRating = 0;
                    dto.ReviewsCount = 0;
                }
            }

            return hotelDtos;
        }

        public override async Task<IEnumerable<HotelDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var allHotels = await _hotelRepository.GetAllAsync();
            var pagedHotels = allHotels.Skip(skip).Take(pageSize);
            var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(pagedHotels).ToList();

            var hotelIds = hotelDtos.Select(h => h.Id).ToHashSet();
            var reviews = await _reviewService.GetAllAsync();
            var grouped = reviews
                .Where(r => hotelIds.Contains(r.HotelId) && !r.IsDeleted && r.IsApproved)
                .GroupBy(r => r.HotelId)
                .ToDictionary(g => g.Key, g => new { Avg = g.Average(x => (double)x.Rating), Cnt = g.Count() });

            foreach (var dto in hotelDtos)
            {
                if (grouped.TryGetValue(dto.Id, out var agg))
                {
                    dto.AverageRating = Math.Round(agg.Avg, 2);
                    dto.ReviewsCount = agg.Cnt;
                }
                else
                {
                    dto.AverageRating = 0;
                    dto.ReviewsCount = 0;
                }
            }

            return hotelDtos;
        }

        public async Task<HotelDto?> GetHotelByIdAsync(int id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null) return null;
            var dto = _mapper.Map<HotelDto>(hotel);

            var hotelReviews = (await _reviewService.GetAllAsync())
                .Where(r => r.HotelId == id && !r.IsDeleted && r.IsApproved)
                .ToList();
            if (hotelReviews.Count > 0)
            {
                dto.AverageRating = Math.Round(hotelReviews.Average(r => (double)r.Rating), 2);
                dto.ReviewsCount = hotelReviews.Count;
            }
            else
            {
                dto.AverageRating = 0;
                dto.ReviewsCount = 0;
            }
            return dto;
        }

        public async Task<IEnumerable<HotelDto>> GetHotelsByCityAsync(string city)
        {
            var hotels = await _hotelRepository.GetHotelsByCityAsync(city);
            var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels).ToList();

            var hotelIds = hotelDtos.Select(h => h.Id).ToHashSet();
            var reviews = await _reviewService.GetAllAsync();
            var grouped = reviews
                .Where(r => hotelIds.Contains(r.HotelId) && !r.IsDeleted && r.IsApproved)
                .GroupBy(r => r.HotelId)
                .ToDictionary(g => g.Key, g => new { Avg = g.Average(x => (double)x.Rating), Cnt = g.Count() });

            foreach (var dto in hotelDtos)
            {
                if (grouped.TryGetValue(dto.Id, out var agg))
                {
                    dto.AverageRating = Math.Round(agg.Avg, 2);
                    dto.ReviewsCount = agg.Cnt;
                }
                else
                {
                    dto.AverageRating = 0;
                    dto.ReviewsCount = 0;
                }
            }

            return hotelDtos;
        }

        public async Task<HotelDto> CreateHotelAsync(CreateHotelDto createHotelDto)
        {
            var hotel = _mapper.Map<Hotel>(createHotelDto);
            var createdHotel = await _hotelRepository.AddAsync(hotel);
            return _mapper.Map<HotelDto>(createdHotel);
        }

        public async Task<bool> UpdateHotelAsync(int id, UpdateHotelDto updateHotelDto)
        {
            var existingHotel = await _hotelRepository.GetByIdAsync(id);
            if (existingHotel == null)
                return false;

            _mapper.Map(updateHotelDto, existingHotel);
            await _hotelRepository.UpdateAsync(existingHotel);
            return true;
        }

        public async Task<bool> DeleteHotelAsync(int id)
        {
            if (!await _hotelRepository.ExistsAsync(id))
                return false;

            await _hotelRepository.DeleteAsync(id);
            return true;
        }

        public async Task<double> GetAverageRatingAsync(int hotelId)
        {
            var reviews = await _reviewService.GetAllAsync();
            var hotelReviews = reviews.Where(r => r.HotelId == hotelId && !r.IsDeleted && r.IsApproved).ToList();
            if (hotelReviews.Count == 0) return 0;
            return Math.Round(hotelReviews.Average(r => (double)r.Rating), 2);
        }

        /// <summary>
        /// User-based collaborative filtering hotel recommendations
        /// </summary>
        /// <summary>
        /// User-based collaborative filtering hotel recommendations with improvements:
        /// - Dynamic rating threshold based on user's average rating
        /// - Time-based weighting for recent reviews
        /// - Configurable number of recommendations
        /// </summary>
        public async Task<IEnumerable<HotelDto>> GetUserBasedHotelRecommendationsAsync(int userId, int maxRecommendations = 3)
        {
            var allReviews = await _reviewService.GetAllAsync();
            var allHotels = await _hotelRepository.GetAllAsync();

            // Reviews of current user
            var userReviews = allReviews.Where(r => r.UserId == userId && r.IsApproved && !r.IsDeleted).ToList();
            var userHotelIds = userReviews.Select(r => r.HotelId).Distinct().ToList();

            // Calculate user's average rating for dynamic threshold
            var userAverageRating = userReviews.Count > 0 ? userReviews.Average(r => r.Rating) : 3.0;
            var dynamicThreshold = Math.Max(3.0, userAverageRating - 0.5); // Minimum threshold of 3.0

            // Find similar users (who rated same hotels)
            var similarUsers = allReviews.Where(r => userHotelIds.Contains(r.HotelId) && r.UserId != userId)
                                        .Select(r => r.UserId)
                                        .Distinct()
                                        .ToList();

            // For each similar user, get their reviews with time-based weighting
            var similarUserReviews = allReviews.Where(r => similarUsers.Contains(r.UserId ?? 0) && r.IsApproved && !r.IsDeleted).ToList();

            // Calculate time-based weights (recent reviews get higher weight)
            var now = DateTime.UtcNow;
            var timeWeightedReviews = similarUserReviews.Select(r => new
            {
                Review = r,
                TimeWeight = CalculateTimeWeight(r.ReviewDate, now)
            }).ToList();

            // Calculate weighted average rating per hotel by similar users
            var recommendedHotels = timeWeightedReviews
                .Where(r => !userHotelIds.Contains(r.Review.HotelId) && r.Review.Rating >= dynamicThreshold)
                .GroupBy(r => r.Review.HotelId)
                .Select(g => new 
                { 
                    HotelId = g.Key, 
                    WeightedAvgRating = g.Sum(r => r.Review.Rating * r.TimeWeight) / g.Sum(r => r.TimeWeight),
                    Count = g.Count(),
                    TotalWeight = g.Sum(r => r.TimeWeight)
                })
                .OrderByDescending(h => h.WeightedAvgRating)
                .ThenByDescending(h => h.TotalWeight) // Prioritize hotels with more recent reviews
                .ThenByDescending(h => h.Count)
                .Take(maxRecommendations)
                .ToList();

            // Get hotel entities
            var hotels = allHotels.Where(h => recommendedHotels.Select(r => r.HotelId).Contains(h.Id)).ToList();

            // Fallback: ako nema sličnih korisnika ili preporuka, vrati top hotele po prosječnom ratingu
            if (hotels.Count == 0)
            {
                var topByRating = allReviews
                    .Where(r => r.IsApproved && !r.IsDeleted)
                    .GroupBy(r => r.HotelId)
                    .Select(g => new { HotelId = g.Key, Avg = g.Average(x => (double)x.Rating), Cnt = g.Count() })
                    .Where(x => x.Avg >= dynamicThreshold)
                    .OrderByDescending(x => x.Avg)
                    .ThenByDescending(x => x.Cnt)
                    .Take(maxRecommendations)
                    .Select(x => x.HotelId)
                    .ToHashSet();

                hotels = allHotels.Where(h => topByRating.Contains(h.Id)).ToList();
            }

            return _mapper.Map<IEnumerable<HotelDto>>(hotels);
        }

        /// <summary>
        /// Calculate time-based weight for reviews. Recent reviews get higher weight.
        /// Weight decreases exponentially with time (half-life of 6 months).
        /// </summary>
        private double CalculateTimeWeight(DateTime reviewDate, DateTime currentDate)
        {
            var daysSinceReview = (currentDate - reviewDate).TotalDays;
            var halfLifeDays = 180.0; // 6 months
            
            // Exponential decay: weight = 2^(-days/halfLife)
            var weight = Math.Pow(2, -daysSinceReview / halfLifeDays);
            
            // Ensure minimum weight of 0.1 for very old reviews
            return Math.Max(0.1, weight);
        }

        public async Task<HotelStatistics> GetHotelStatisticsAsync()
        {
            try
            {
                var hotels = await _hotelRepository.GetAllAsync();
                var rooms = await _roomRepository.GetAllAsync();
                var reviews = await _reviewService.GetAllAsync();
                var bookings = await _bookingRepository.GetAllAsync();

                var activeHotels = hotels.Where(h => !h.IsDeleted).ToList();
                var totalHotels = activeHotels.Count;
                var totalRooms = rooms.Where(r => !r.IsDeleted).Count();
                var availableRooms = rooms.Where(r => !r.IsDeleted && r.IsAvailable).Count();

                // Calculate average rating across all hotels
                var approvedReviews = reviews.Where(r => r.IsApproved && !r.IsDeleted).ToList();
                var averageRating = approvedReviews.Count > 0 ? approvedReviews.Average(r => (double)r.Rating) : 0;

                // Top hotels by revenue and bookings
                var topHotels = new List<TopHotelData>();
                foreach (var hotel in activeHotels.Take(5))
                {
                    var hotelBookings = bookings.Where(b => !b.IsDeleted).ToList();
                    var hotelRevenue = hotelBookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedOut)
                        .Sum(b => b.TotalPrice);
                    var hotelRooms = rooms.Where(r => r.HotelId == hotel.Id && !r.IsDeleted).Count();
                    var hotelReviews = reviews.Where(r => r.HotelId == hotel.Id && r.IsApproved && !r.IsDeleted).ToList();
                    var hotelRating = hotelReviews.Count > 0 ? hotelReviews.Average(r => (double)r.Rating) : 0;

                    topHotels.Add(new TopHotelData
                    {
                        HotelId = hotel.Id,
                        Name = hotel.Name,
                        AverageRating = Math.Round(hotelRating, 2),
                        TotalBookings = hotelBookings.Count,
                        TotalRevenue = hotelRevenue,
                        OccupancyRate = 0.0 // TODO: Calculate actual occupancy
                    });
                }

                // Sort by revenue
                topHotels = topHotels.OrderByDescending(h => h.AverageRating).ToList();

                // Occupancy data
                var occupancyData = new List<HotelOccupancyData>();
                foreach (var hotel in activeHotels)
                {
                    var hotelRooms = rooms.Where(r => r.HotelId == hotel.Id && !r.IsDeleted).ToList();
                    var totalHotelRooms = hotelRooms.Count;
                    var occupiedRooms = hotelRooms.Count(r => !r.IsAvailable);
                    var occupancyRate = totalHotelRooms > 0 ? (double)occupiedRooms / totalHotelRooms * 100 : 0;

                    occupancyData.Add(new HotelOccupancyData
                    {
                        HotelId = hotel.Id,
                        HotelName = hotel.Name,
                        OccupancyRate = Math.Round(occupancyRate, 2),
                        TotalRooms = totalHotelRooms,
                        OccupiedRooms = occupiedRooms
                    });
                }

                return new HotelStatistics
                {
                    TotalHotels = totalHotels,
                    ActiveHotels = totalHotels,
                    AverageRating = Math.Round(averageRating, 2),
                    TotalRooms = totalRooms,
                    AvailableRooms = availableRooms,
                    TopHotels = topHotels,
                    OccupancyData = occupancyData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating hotel statistics");
                throw;
            }
        }

        public async Task<IEnumerable<HotelDto>> GetHotelsByNameAsync(string name)
        {
            var hotels = await _hotelRepository.GetHotelsByNameAsync(name);
            var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(hotels).ToList();

            var hotelIds = hotelDtos.Select(h => h.Id).ToHashSet();
            var reviews = await _reviewService.GetAllAsync();
            var grouped = reviews
                .Where(r => hotelIds.Contains(r.HotelId) && !r.IsDeleted && r.IsApproved)
                .GroupBy(r => r.HotelId)
                .ToDictionary(g => g.Key, g => new { Avg = g.Average(x => (double)x.Rating), Cnt = g.Count() });

            foreach (var dto in hotelDtos)
            {
                if (grouped.TryGetValue(dto.Id, out var agg))
                {
                    dto.AverageRating = Math.Round(agg.Avg, 2);
                    dto.ReviewsCount = agg.Cnt;
                }
                else
                {
                    dto.AverageRating = 0;
                    dto.ReviewsCount = 0;
                }
            }

            return hotelDtos;
        }
    }
}
