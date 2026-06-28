using CreditDefault.Api.Hubs;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace CreditDefault.Api.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cache;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public NotificationService(
            INotificationRepository repository,
            IUserRepository userRepository,
            ICacheService cache,
            IHubContext<NotificationsHub> hubContext)
        {
            _repository = repository;
            _userRepository = userRepository;
            _cache = cache;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetForUserAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var key = CacheKeys.NotificationsList(userId, page, pageSize);
            var cached = await _cache.GetAsync<List<Notification>>(key);
            if (cached != null) return cached;

            var notifications = await _repository.GetForUserAsync(userId, page, pageSize);
            await _cache.SetAsync(key, notifications, TimeSpan.FromMinutes(2));
            return notifications;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var key = CacheKeys.NotificationsUnread(userId);
            var cached = await _cache.GetAsync<int?>(key);
            if (cached.HasValue) return cached.Value;

            var count = await _repository.GetUnreadCountAsync(userId);
            await _cache.SetAsync(key, count, TimeSpan.FromMinutes(2));
            return count;
        }

        public async Task<Notification> CreateNotificationAsync(Guid userId, string type, string title, string message)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);
            await InvalidateNotificationCacheAsync(userId);
            await SendNotificationAsync(userId, notification);
            return notification;
        }

        public async Task NotifyPredictionCompletedAsync(Guid userId, Guid predictionId, string riskLevel, int riskScore)
        {
            var notification = await CreateNotificationAsync(
                userId,
                "PredictionCompleted",
                "Credit Risk Prediction Completed",
                $"Your prediction was completed with risk level: {riskLevel} and score: {riskScore}%.");

            await _hubContext.Clients.Group(NotificationsHub.UserGroup(userId.ToString()))
                .SendAsync("PredictionCompleted", new
                {
                    predictionId,
                    riskLevel,
                    riskScore,
                    notification
                });
        }

        public async Task NotifyAdminAsync(string type, string title, string message)
        {
            var users = await _userRepository.GetAllAsync();
            var admins = users.Where(u => u.Role == "Admin" || u.UserRoles.Any(ur => ur.Role.Name == "Admin")).ToList();

            foreach (var admin in admins)
            {
                await CreateNotificationAsync(admin.Id, type, title, message);
            }

            await _hubContext.Clients.Group(NotificationsHub.RoleGroup("Admin"))
                .SendAsync(type, new { type, title, message, createdAt = DateTime.UtcNow });
        }

        public async Task<bool> MarkReadAsync(Guid userId, Guid id)
        {
            var notification = await _repository.GetForUserByIdAsync(userId, id);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(notification);
            await InvalidateNotificationCacheAsync(userId);
            await SendUnreadCountAsync(userId);
            return true;
        }

        public async Task MarkAllReadAsync(Guid userId)
        {
            await _repository.MarkAllReadAsync(userId);
            await InvalidateNotificationCacheAsync(userId);
            await SendUnreadCountAsync(userId);
        }

        private async Task SendNotificationAsync(Guid userId, Notification notification)
        {
            await _hubContext.Clients.Group(NotificationsHub.UserGroup(userId.ToString()))
                .SendAsync("NotificationReceived", notification);
            await SendUnreadCountAsync(userId);
        }

        private async Task SendUnreadCountAsync(Guid userId)
        {
            var count = await GetUnreadCountAsync(userId);
            await _hubContext.Clients.Group(NotificationsHub.UserGroup(userId.ToString()))
                .SendAsync("UnreadCountUpdated", count);
        }

        private async Task InvalidateNotificationCacheAsync(Guid userId)
        {
            await _cache.RemoveAsync(CacheKeys.NotificationsUnread(userId));
            for (var page = 1; page <= 5; page++)
            {
                await _cache.RemoveAsync(CacheKeys.NotificationsList(userId, page, 20));
            }
        }
    }
}
