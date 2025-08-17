using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface INotificationService : IBaseService<NotificationDto, CreateNotificationDto, UpdateNotificationDto>
    {
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync();
    }
}
