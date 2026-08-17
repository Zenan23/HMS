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
        private readonly IFileStorageService _fileStorage;
        private readonly IRepository<HotelViewHistory> _hotelViewHistoryRepository;

        public HotelService(
            IHotelRepository hotelRepository,
            IReviewService reviewService,
            IRoomService roomService,
            IBookingService bookingService,
            IRepository<Room> roomRepository,
            IRepository<Booking> bookingRepository,
            IFileStorageService fileStorage,
            IRepository<HotelViewHistory> hotelViewHistoryRepository,
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
            _fileStorage = fileStorage;
            _hotelViewHistoryRepository = hotelViewHistoryRepository;
        }

        public async Task RecordHotelViewAsync(int userId, int hotelId)
        {
            try
            {
                await _hotelViewHistoryRepository.AddAsync(new HotelViewHistory
                {
                    UserId = userId,
                    HotelId = hotelId,
                    ViewedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Best-effort: greška pri upisu pregleda ne smije spriječiti prikaz hotela korisniku.
                _logger.LogWarning(ex, "Nije uspjelo bilježenje pregleda hotela {HotelId} za korisnika {UserId}", hotelId, userId);
            }
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
                hotels = hotels.Where(h => h.City != null && h.City.Name.Contains(city, StringComparison.OrdinalIgnoreCase));  // Filtriraj prema gradu
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
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            var skip = (pageNumber - 1) * pageSize;
            // Prava DB-level paginacija (Skip/Take u SQL-u) — prije je ovdje bila učitana CIJELA
            // tabela hotela pa tek onda rezana u memoriji ("lažna paginacija").
            var pagedHotels = await _hotelRepository.GetPagedAsync(skip, pageSize);
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
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
                return false;

            if (!string.IsNullOrWhiteSpace(hotel.ImageUrl))
                await _fileStorage.DeleteHotelImageAsync(hotel.ImageUrl);

            // Soft delete — hard delete (Repository<T>.DeleteAsync) je zabranjen uputama za
            // entitete koji učestvuju u poslovnim relacijama (sobe, rezervacije...).
            hotel.IsDeleted = true;
            hotel.UpdatedAt = DateTime.UtcNow;
            await _hotelRepository.UpdateAsync(hotel);
            return true;
        }

        public async Task<HotelDto?> SetHotelImageAsync(int hotelId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
        {
            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            if (hotel == null)
                return null;

            if (!string.IsNullOrWhiteSpace(hotel.ImageUrl))
                await _fileStorage.DeleteHotelImageAsync(hotel.ImageUrl);

            hotel.ImageUrl = await _fileStorage.SaveHotelImageAsync(hotelId, fileStream, fileName, cancellationToken);
            hotel.UpdatedAt = DateTime.UtcNow;
            await _hotelRepository.UpdateAsync(hotel);

            return await GetHotelByIdAsync(hotelId);
        }

        public async Task<bool> RemoveHotelImageAsync(int hotelId, CancellationToken cancellationToken = default)
        {
            var hotel = await _hotelRepository.GetByIdAsync(hotelId);
            if (hotel == null)
                return false;

            await _fileStorage.DeleteHotelImageAsync(hotel.ImageUrl);
            hotel.ImageUrl = string.Empty;
            hotel.UpdatedAt = DateTime.UtcNow;
            await _hotelRepository.UpdateAsync(hotel);
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
            var dtos = _mapper.Map<List<HotelDto>>(hotels);

            if (dtos.Count > 0)
            {
                // Objašnjiva preporuka (user-based collaborative filtering grana): korisniku se
                // navodi na osnovu čega je hotel preporučen — broj sličnih korisnika i njihova
                // vremenski ponderisana prosječna ocjena za taj hotel.
                var reasonByHotelId = recommendedHotels.ToDictionary(r => r.HotelId, r => r);
                foreach (var dto in dtos)
                {
                    if (reasonByHotelId.TryGetValue(dto.Id, out var r))
                    {
                        dto.RecommendationReason =
                            $"Preporučeno jer su korisnici sličnih preferencija ovaj hotel ocijenili prosječnom ocjenom {r.WeightedAvgRating:F1}/5 ({r.Count} {(r.Count == 1 ? "recenzija" : "recenzije")}).";
                    }
                }
                return dtos;
            }

            // Fallback: ako nema sličnih korisnika ili preporuka, vrati top hotele po prosječnom ratingu
            // (popularity-based pristup). Broj STVARNIH pregleda (HotelViewHistory — upisuje se u
            // HotelsController.GetById pri svakom pregledu detalja hotela) se ovdje zaista koristi
            // kao dodatni popularity signal za rangiranje, ne samo prikuplja bez svrhe.
            var allViews = await _hotelViewHistoryRepository.GetAllAsync();
            var viewCountByHotelId = allViews
                .GroupBy(v => v.HotelId)
                .ToDictionary(g => g.Key, g => g.Count());

            var topByRating = allReviews
                .Where(r => r.IsApproved && !r.IsDeleted)
                .GroupBy(r => r.HotelId)
                .Select(g => new
                {
                    HotelId = g.Key,
                    Avg = g.Average(x => (double)x.Rating),
                    Cnt = g.Count(),
                    Views = viewCountByHotelId.TryGetValue(g.Key, out var v) ? v : 0
                })
                .Where(x => x.Avg >= dynamicThreshold)
                .OrderByDescending(x => x.Avg)
                .ThenByDescending(x => x.Views) // trending: više pregleda = veći prioritet kod izjednačene ocjene
                .ThenByDescending(x => x.Cnt)
                .Take(maxRecommendations)
                .ToList();

            var fallbackHotels = allHotels.Where(h => topByRating.Select(x => x.HotelId).Contains(h.Id)).ToList();
            var fallbackDtos = _mapper.Map<List<HotelDto>>(fallbackHotels);
            var fallbackReasonByHotelId = topByRating.ToDictionary(x => x.HotelId, x => x);
            foreach (var dto in fallbackDtos)
            {
                if (fallbackReasonByHotelId.TryGetValue(dto.Id, out var x))
                {
                    dto.RecommendationReason = x.Views > 0
                        ? $"Trenutno jedan od najbolje ocijenjenih i najgledanijih hotela — prosječna ocjena {x.Avg:F1}/5 iz {x.Cnt} {(x.Cnt == 1 ? "recenzije" : "recenzija")}, {x.Views} {(x.Views == 1 ? "pregled" : "pregleda")}."
                        : $"Trenutno jedan od najbolje ocijenjenih hotela — prosječna ocjena {x.Avg:F1}/5 iz {x.Cnt} {(x.Cnt == 1 ? "recenzije" : "recenzija")}.";
                }
            }
            return fallbackDtos;
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
