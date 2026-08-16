using Application.Helpers;
using AutoMapper;
using Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class PriceAdjustmentService : BaseDtoService<PriceAdjustment, PriceAdjustmentDto, CreatePriceAdjustmentDto, UpdatePriceAdjustmentDto>, IPriceAdjustmentService
    {
        private readonly ApplicationDbContext _context;

        public PriceAdjustmentService(
            IRepository<PriceAdjustment> repository,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<PriceAdjustmentService> logger)
            : base(repository, mapper, logger)
        {
            _context = context;
        }

        // Generički Repository<T>.GetAllAsync() ne radi .Include(), pa bi CreatedByUser/Hotel
        // navigacije (i time CreatedByUserName/HotelName u DTO-u) ostale prazne bez ovoga.
        private IQueryable<PriceAdjustment> QueryWithIncludes() =>
            _context.PriceAdjustments
                .Include(x => x.CreatedByUser)
                .Include(x => x.Hotel);

        public override async Task<PriceAdjustmentDto?> GetByIdAsync(int id)
        {
            var entity = await QueryWithIncludes().FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : _mapper.Map<PriceAdjustmentDto>(entity);
        }

        public override async Task<IEnumerable<PriceAdjustmentDto>> GetAllAsync()
        {
            var entities = await QueryWithIncludes().ToListAsync();
            return _mapper.Map<IEnumerable<PriceAdjustmentDto>>(entities);
        }

        public override async Task<IEnumerable<PriceAdjustmentDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            var entities = await QueryWithIncludes()
                .OrderByDescending(x => x.StartDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<IEnumerable<PriceAdjustmentDto>>(entities);
        }

        /// <param name="hotelId">Ako je zadano, vraća globalne (HotelId == null) I one specifične za taj hotel.</param>
        public async Task<IEnumerable<PriceAdjustmentDto>> GetActiveAdjustmentsAsync(DateTime atDate, int? hotelId = null)
        {
            var entities = await QueryWithIncludes().Where(x =>
                x.StartDate <= atDate &&
                x.EndDate >= atDate &&
                !x.IsDeleted &&
                (x.HotelId == null || x.HotelId == hotelId)).ToListAsync();
            return _mapper.Map<IEnumerable<PriceAdjustmentDto>>(entities);
        }

        /// <param name="hotelId">Soba/hotel za koji se cijena računa — filtrira globalne + hotel-specifične adjustmente.</param>
        public async Task<decimal> ApplyActiveAdjustmentsAsync(decimal basePrice, DateTime atDate, int? hotelId = null)
        {
            var active = await _context.PriceAdjustments.Where(x =>
                x.StartDate <= atDate &&
                x.EndDate >= atDate &&
                !x.IsDeleted &&
                (x.HotelId == null || x.HotelId == hotelId)).ToListAsync();
            return PriceAdjustmentCalculator.Apply(basePrice, active);
        }
    }
}
