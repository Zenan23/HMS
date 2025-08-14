using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class NotificationDto : BaseEntityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? BookingId { get; set; }
    }

    public class CreateNotificationDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; } = "Normal";

        [Url(ErrorMessage = "Action URL must be a valid URL")]
        public string? ActionUrl { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? BookingId { get; set; }
    }

    public class UpdateNotificationDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; } = "Normal";

        [Url(ErrorMessage = "Action URL must be a valid URL")]
        public string? ActionUrl { get; set; }
    }

}
