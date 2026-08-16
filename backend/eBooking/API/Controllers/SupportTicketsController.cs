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

            if (!IsSelfOrElevated(userId))
            {
                return Forbid();
            }

            var tickets = await _supportTicketService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<SupportTicketDto>>.SuccessResult(tickets, "Support tickets retrieved successfully."));
        }

        [HttpGet("status/{status}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupportTicketDto>>>> GetByStatus([FromRoute] SupportTicketStatus status)
        {
            var tickets = await _supportTicketService.GetByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<SupportTicketDto>>.SuccessResult(tickets, "Support tickets retrieved successfully."));
        }

        // Lista SVIH tiketa (bez filtera) — samo osoblje/admin, sadrži tuđe podatke.
        [HttpGet]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public override Task<ActionResult<ApiResponse<PaginatedResult<SupportTicketDto>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => base.GetAll(pageNumber, pageSize);

        // Pojedinačni tiket po ID-u — spriječi da korisnik pogodi/enumeriše tuđi ticketId.
        [HttpGet("{id}")]
        public override async Task<ActionResult<ApiResponse<SupportTicketDto>>> GetById([FromRoute] int id)
        {
            var result = await base.GetById(id);
            if (result.Result is OkObjectResult ok && ok.Value is ApiResponse<SupportTicketDto> resp && resp.Data != null)
            {
                if (!IsSelfOrElevated(resp.Data.UserId))
                {
                    return Forbid();
                }
            }
            return result;
        }
    }
}
