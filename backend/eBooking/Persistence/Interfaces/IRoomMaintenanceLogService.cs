using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IRoomMaintenanceLogService : IBaseService<RoomMaintenanceLogDto, CreateRoomMaintenanceLogDto, UpdateRoomMaintenanceLogDto>
    {
        Task<IEnumerable<RoomMaintenanceLogDto>> GetByRoomIdAsync(int roomId);
    }
}
