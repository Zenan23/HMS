using Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class SupportTicketDto : BaseEntityDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public SupportTicketStatus Status { get; set; }
        public SupportTicketPriority Priority { get; set; }
    }

    public class CreateSupportTicketDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message body is required")]
        [StringLength(5000, ErrorMessage = "Message body cannot exceed 5000 characters")]
        public string MessageBody { get; set; } = string.Empty;

        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
        public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Medium;
    }

    public class UpdateSupportTicketDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message body is required")]
        [StringLength(5000, ErrorMessage = "Message body cannot exceed 5000 characters")]
        public string MessageBody { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public SupportTicketStatus Status { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public SupportTicketPriority Priority { get; set; }
    }
}
