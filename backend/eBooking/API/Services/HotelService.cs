using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;

namespace API.Services
{
    public class HotelService : BaseDtoService<Hotel,HotelDto,CreateHotelDto,UpdateHotelDto>, IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IReviewService _reviewService;

        public HotelService(
            IHotelRepository hotelRepository,
            IReviewService reviewService,
            IMapper mapper,
            ILogger<HotelService> logger)
            : base(hotelRepository, mapper, logger)
        {
            _hotelRepository = hotelRepository;
            _reviewService = reviewService;
        }

        public async Task<IEnumerable<HotelDto>> GetAllHotelsAsync()
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
    }
}
