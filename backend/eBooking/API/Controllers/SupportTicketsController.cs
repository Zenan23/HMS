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
    public class SupportTicketsController : BaseController<SupportTicketDto, CreateSupportTicketDto, UpdateSupportTicketDto>
    {
        private readonly ISupportTicketService _supportTicketService;

        public SupportTicketsController(
            ISupportTicketService supportTicketService,
            ILogger<SupportTicketsController> logger)
            : base(supportTicketService, logger)
        {
            _supportTicketService = supportTicketService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupportTicketDto>>>> GetByUserId([FromRoute] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<SupportTicketDto>>.ErrorResult("Invalid user ID."));
            }

            var tickets = await _supportTicketService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<SupportTicketDto>>.SuccessResult(tickets, "Support tickets retrieved successfully."));
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupportTicketDto>>>> GetByStatus([FromRoute] SupportTicketStatus status)
        {
            var tickets = await _supportTicketService.GetByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<SupportTicketDto>>.SuccessResult(tickets, "Support tickets retrieved successfully."));
        }
    }
}
