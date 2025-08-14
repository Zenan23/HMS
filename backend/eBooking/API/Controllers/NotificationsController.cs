using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : BaseController<NotificationDto, CreateNotificationDto, UpdateNotificationDto>
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
            : base(notificationService, logger)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get notifications by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of notifications</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetByUserId([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<IEnumerable<NotificationDto>>.ErrorResult("Invalid user ID."));
                }

                var notifications = await _notificationService.GetByUserIdAsync(userId);
                return Ok(ApiResponse<IEnumerable<NotificationDto>>.SuccessResult(notifications, "Notifications retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<IEnumerable<NotificationDto>>.ErrorResult("An error occurred while retrieving notifications."));
            }
        }

        /// <summary>
        /// Get unread notifications
        /// </summary>
        /// <returns>List of unread notifications</returns>
        [HttpGet("unread")]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetUnreadNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync();
                return Ok(ApiResponse<IEnumerable<NotificationDto>>.SuccessResult(notifications, "Unread notifications retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications");
                return StatusCode(500, ApiResponse<IEnumerable<NotificationDto>>.ErrorResult("An error occurred while retrieving unread notifications."));
            }
        }

        /// <summary>
        /// Get unread notifications count for a user
        /// </summary>
        [HttpGet("unread-count/{userId}")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount([FromRoute] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(ApiResponse<int>.ErrorResult("Invalid user ID."));
                }

                var unread = await _notificationService.GetUnreadNotificationsAsync();
                var count = unread.Count(n => n.UserId == userId);
                return Ok(ApiResponse<int>.SuccessResult(count, "Unread count retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread count for user ID: {UserId}", userId);
                return StatusCode(500, ApiResponse<int>.ErrorResult("An error occurred while retrieving unread count."));
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Update result</returns>
        [HttpPatch("{id}/mark-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResult("Invalid notification ID."));
                }

                var notification = await _notificationService.GetByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResult($"Notification with ID {id} not found."));
                }

                var updateDto = new UpdateNotificationDto
                {
                    Id = id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    IsRead = true,
                    Priority = notification.Priority,
                    ActionUrl = notification.ActionUrl
                };

                var result = await _notificationService.UpdateAsync(id, updateDto);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Notification marked as read successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read with ID: {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResult("An error occurred while marking notification as read."));
            }
        }
    }
}
