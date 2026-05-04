using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class RoomMaintenanceLogService : BaseDtoService<RoomMaintenanceLog, RoomMaintenanceLogDto, CreateRoomMaintenanceLogDto, UpdateRoomMaintenanceLogDto>, IRoomMaintenanceLogService
    {
        public RoomMaintenanceLogService(
            IRepository<RoomMaintenanceLog> repository,
            IMapper mapper,
            ILogger<RoomMaintenanceLogService> logger)
            : base(repository, mapper, logger)
        {
        }

        public async Task<IEnumerable<RoomMaintenanceLogDto>> GetByRoomIdAsync(int roomId)
        {
            var entities = await _repository.GetAllAsync();
            var filtered = entities
                .Where(x => x.RoomId == roomId && !x.IsDeleted)
                .OrderByDescending(x => x.ReportedAt);
            return _mapper.Map<IEnumerable<RoomMaintenanceLogDto>>(filtered);
        }
    }
}
