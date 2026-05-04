using Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomMaintenanceLogsController : BaseController<RoomMaintenanceLogDto, CreateRoomMaintenanceLogDto, UpdateRoomMaintenanceLogDto>
    {
        private readonly IRoomMaintenanceLogService _roomMaintenanceLogService;

        public RoomMaintenanceLogsController(
            IRoomMaintenanceLogService roomMaintenanceLogService,
            ILogger<RoomMaintenanceLogsController> logger)
            : base(roomMaintenanceLogService, logger)
        {
            _roomMaintenanceLogService = roomMaintenanceLogService;
        }

        [HttpGet("room/{roomId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoomMaintenanceLogDto>>>> GetByRoomId([FromRoute] int roomId)
        {
            if (roomId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<RoomMaintenanceLogDto>>.ErrorResult("Invalid room ID."));
            }

            var logs = await _roomMaintenanceLogService.GetByRoomIdAsync(roomId);
            return Ok(ApiResponse<IEnumerable<RoomMaintenanceLogDto>>.SuccessResult(logs, "Maintenance logs retrieved successfully."));
        }
    }
}
