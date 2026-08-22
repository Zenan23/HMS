using API.Attributes;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupportTicketsController : BaseController<SupportTicketDto, CreateSupportTicketDto, UpdateSupportTicketDto>
    {
        private readonly ISupportTicketService _supportTicketService;
        private readonly INotificationService _notificationService;

        public SupportTicketsController(
            ISupportTicketService supportTicketService,
            INotificationService notificationService,
            ILogger<SupportTicketsController> logger)
            : base(supportTicketService, logger)
        {
            _supportTicketService = supportTicketService;
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupportTicketDto>>>> GetByUserId([FromRoute] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<SupportTicketDto>>.ErrorResult("Nevažeći ID korisnika."));
            }

            if (!IsSelfOrElevated(userId))
            {
                return Forbid();
            }

            var tickets = await _supportTicketService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<SupportTicketDto>>.SuccessResult(tickets, "Tiketi podrške su uspješno učitani."));
        }

        [HttpGet("status/{status}")]
        [AuthorizeRole(UserRole.Employee, UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupportTicketDto>>>> GetByStatus([FromRoute] SupportTicketStatus status)
        {
            var tickets = await _supportTicketService.GetByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<SupportTicketDto>>.SuccessResult(tickets, "Tiketi podrške su uspješno učitani."));
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

        /// <summary>
        /// Ažuriranje tiketa. Gost smije urediti SAMO svoj tiket i SAMO Subject/MessageBody —
        /// Status/Priority/AdminResponse su isključivo posao osoblja (prije ove izmjene nije
        /// postojala nikakva provjera vlasništva ovdje, bilo koji ulogovani korisnik je mogao
        /// PUT-ovati tuđi tiket). Kad Employee/Admin postavi AdminResponse, RespondedAt/
        /// RespondedByUserId se stamp-uju server-side i gost dobija Notification.
        /// </summary>
        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<SupportTicketDto>>> Update([FromRoute] int id, [FromBody] UpdateSupportTicketDto updateDto)
        {
            var existing = await _supportTicketService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<SupportTicketDto>.ErrorResult($"Tiket sa ID {id} nije pronađen."));
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var isStaff = roleClaim != null &&
                (roleClaim.Value == UserRole.Employee.ToString() || roleClaim.Value == UserRole.Admin.ToString());

            if (!isStaff)
            {
                if (!IsSelfOrElevated(existing.UserId))
                {
                    return Forbid();
                }

                // Gost ne smije mijenjati Status/Priority/AdminResponse kroz svoj profil.
                updateDto.Status = existing.Status;
                updateDto.Priority = existing.Priority;
                updateDto.AdminResponse = existing.AdminResponse;
            }

            var newResponseText = updateDto.AdminResponse?.Trim();
            var isNewResponse = isStaff
                && !string.IsNullOrWhiteSpace(newResponseText)
                && !string.Equals(newResponseText, existing.AdminResponse, StringComparison.Ordinal);

            var result = await base.Update(id, updateDto);

            if (isNewResponse && result.Result is OkObjectResult)
            {
                var uidClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (uidClaim != null && int.TryParse(uidClaim.Value, out var staffUserId))
                {
                    await _supportTicketService.SetResponseMetadataAsync(id, staffUserId);
                }

                try
                {
                    await _notificationService.CreateAsync(new CreateNotificationDto
                    {
                        UserId = existing.UserId,
                        Title = "Odgovor na vaš tiket",
                        Message = $"Dobili ste odgovor na tiket \"{existing.Subject}\".",
                        Type = "SupportTicketResponded",
                        Priority = "Normal"
                    });
                }
                catch (Exception ex)
                {
                    // Notifikacija je best-effort — ne smije srušiti uspješno sačuvan odgovor.
                    _logger.LogError(ex, "Failed to create notification for support ticket response, ticket ID: {Id}", id);
                }
            }

            return result;
        }
    }
}
