namespace CreditDefault.Api.Services
{
    public static class CacheKeys
    {
        public static string DashboardUser(Guid userId) => $"dashboard:user:{userId}";
        public const string DashboardAdmin = "dashboard:admin";
        public static string NotificationsUnread(Guid userId) => $"notifications:unread:{userId}";
        public static string NotificationsList(Guid userId, int page, int pageSize) => $"notifications:list:{userId}:page:{page}:size:{pageSize}";
        public static string PredictionsRecent(Guid userId, bool isAdmin, int limit) => isAdmin ? $"predictions:recent:admin:{limit}" : $"predictions:recent:{userId}:{limit}";
    }
}
