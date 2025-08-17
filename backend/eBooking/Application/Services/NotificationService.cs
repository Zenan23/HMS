using AutoMapper;
using Contracts.DTOs;
using Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;


namespace Application.Services
{
    public class NotificationService : BaseDtoService<Notification, NotificationDto, CreateNotificationDto, UpdateNotificationDto>, INotificationService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public NotificationService(
            IRepository<Notification> repository,
            IMapper mapper,
            ILogger<NotificationService> logger,
            IPublishEndpoint publishEndpoint)
            : base(repository, mapper, logger)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting notifications for user ID: {UserId}", userId);
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(n => n.UserId == userId && !n.IsDeleted)
                                             .OrderByDescending(n => n.SentDate);
                return _mapper.Map<IEnumerable<NotificationDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Getting unread notifications");
                var entities = await _repository.GetAllAsync();
                var filteredEntities = entities.Where(n => !n.IsRead && !n.IsDeleted)
                                             .OrderByDescending(n => n.SentDate);
                return _mapper.Map<IEnumerable<NotificationDto>>(filteredEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                throw;
            }
        }

        public override async Task<bool> UpdateAsync(int id, UpdateNotificationDto updateDto)
        {
            try
            {
                var result = await base.UpdateAsync(id, updateDto);

                // If marking as read, set the read date
                if (result && updateDto.IsRead)
                {
                    var entity = await _repository.GetByIdAsync(id);
                    if (entity != null && entity.ReadDate == null)
                    {
                        entity.ReadDate = DateTime.UtcNow;
                        await _repository.UpdateAsync(entity);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification with ID: {Id}", id);
                throw;
            }
        }

        public override async Task<NotificationDto> CreateAsync(CreateNotificationDto createDto)
        {
            var dto = await base.CreateAsync(createDto);
            await _publishEndpoint.Publish(new NotificationCreated(dto.Id, dto.UserId));
            return dto;
        }
    }
}
