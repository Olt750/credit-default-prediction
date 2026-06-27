using CreditDefault.Api.Models;

namespace CreditDefault.Api.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetForUserAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<Notification?> GetForUserByIdAsync(Guid userId, Guid id);
        Task MarkAllReadAsync(Guid userId);
        Task UpdateAsync(Notification notification);
    }
}
