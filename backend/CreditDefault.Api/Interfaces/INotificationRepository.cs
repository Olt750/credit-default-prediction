using CreditDefault.Api.Models;

namespace CreditDefault.Api.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetForUserAsync(Guid userId);
        Task<List<Notification>> GetForUserAsync(Guid userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<Notification?> GetForUserByIdAsync(Guid userId, Guid id);
        Task AddAsync(Notification notification);
        Task MarkAllReadAsync(Guid userId);
        Task UpdateAsync(Notification notification);
    }
}
