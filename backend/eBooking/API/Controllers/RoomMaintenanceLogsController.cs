using API.Attributes;
using Contracts.DTOs;
using Contracts.Enums;
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

        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<RoomMaintenanceLogDto>>> Create([FromBody] CreateRoomMaintenanceLogDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<RoomMaintenanceLogDto>>> Update([FromRoute] int id, [FromBody] UpdateRoomMaintenanceLogDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);

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
