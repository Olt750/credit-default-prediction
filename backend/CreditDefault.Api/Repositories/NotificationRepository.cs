using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        public NotificationRepository(AppDbContext context) => _context = context;

        public async Task<List<Notification>> GetForUserAsync(Guid userId) =>
            await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(Guid userId) =>
            await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<Notification?> GetForUserByIdAsync(Guid userId, Guid id) =>
            await _context.Notifications.FirstOrDefaultAsync(n => n.UserId == userId && n.Id == id);

        public async Task MarkAllReadAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var unread = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                notification.UpdatedAt = now;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
    }
}
