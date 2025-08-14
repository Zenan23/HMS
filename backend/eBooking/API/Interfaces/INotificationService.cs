using API.DTOs;

namespace API.Interfaces
{
    public interface INotificationService : IBaseService<NotificationDto, CreateNotificationDto, UpdateNotificationDto>
    {
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync();
        // CreateAsync je već definisan u IBaseService; ne treba redeklaracija
    }
}
