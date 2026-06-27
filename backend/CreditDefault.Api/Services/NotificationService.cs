using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _repository;
        public NotificationService(INotificationRepository repository) => _repository = repository;

        public Task<List<Notification>> GetForUserAsync(Guid userId) => _repository.GetForUserAsync(userId);
        public Task<int> GetUnreadCountAsync(Guid userId) => _repository.GetUnreadCountAsync(userId);

        public async Task<bool> MarkReadAsync(Guid userId, Guid id)
        {
            var notification = await _repository.GetForUserByIdAsync(userId, id);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(notification);
            return true;
        }

        public Task MarkAllReadAsync(Guid userId) => _repository.MarkAllReadAsync(userId);
    }
}
