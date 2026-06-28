import { createFileRoute } from "@tanstack/react-router";
import { AlertTriangle, Info, CheckCircle2, TrendingUp } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { useNotifications } from "@/lib/notifications";
import { searchList } from "@/services/additionalFeaturesApi";
import { useEffect, useState } from "react";

export const Route = createFileRoute("/_app/notifications")({
  component: NotificationsPage,
});

const iconMap = {
  HighRiskAlert: { Icon: AlertTriangle, cls: "bg-destructive/15 text-destructive" },
  PredictionCompleted: { Icon: CheckCircle2, cls: "bg-success/15 text-success" },
  SettingsChanged: { Icon: TrendingUp, cls: "bg-warning/15 text-warning" },
  default: { Icon: Info, cls: "bg-primary/15 text-primary" },
} as const;

function NotificationsPage() {
  const { notifications, unreadCount, loading, markRead, markAllRead } = useNotifications();
  const [items, setItems] = useState<any[]>([]);
  const [keyword, setKeyword] = useState("");
  const [type, setType] = useState("");
  const [isRead, setIsRead] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [searchLoading, setSearchLoading] = useState(false);

  useEffect(() => {
    async function load() {
      setSearchLoading(true);
      try {
        const result = await searchList<any>("notifications", {
          keyword,
          type,
          isRead,
          page,
          pageSize: 10,
          sortBy: "createdAt",
          sortDirection: "desc",
        });
        setItems(result.items);
        setTotalPages(result.totalPages || 1);
      } finally {
        setSearchLoading(false);
      }
    }
    load().catch(() => setItems(notifications));
  }, [keyword, type, isRead, page, notifications]);

  const visibleNotifications = keyword || type || isRead || items.length ? items : notifications;

  return (
    <>
      <PageHeader title="Notifications" subtitle="Real-time credit risk alerts and system updates." />

      <div className="mb-4 flex items-center justify-between gap-3">
        <div className="text-sm text-muted-foreground">
          {unreadCount === 0 ? "No unread notifications" : `${unreadCount} unread notification${unreadCount === 1 ? "" : "s"}`}
        </div>
        <button
          onClick={() => markAllRead().catch(() => undefined)}
          disabled={unreadCount === 0}
          className="h-9 px-3 rounded-lg border border-border text-sm hover:bg-muted/40 disabled:opacity-50 disabled:hover:bg-transparent"
        >
          Mark all as read
        </button>
      </div>

      <div className="glass rounded-2xl p-4 mb-4 flex flex-wrap gap-3">
        <input value={keyword} onChange={(e) => { setKeyword(e.target.value); setPage(1); }} placeholder="Search notifications" className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm flex-1 min-w-56" />
        <select value={type} onChange={(e) => { setType(e.target.value); setPage(1); }} className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
          <option value="">All types</option>
          <option>PredictionCompleted</option>
          <option>HighRiskAlert</option>
          <option>ReportGenerated</option>
          <option>ImportCompleted</option>
        </select>
        <select value={isRead} onChange={(e) => { setIsRead(e.target.value); setPage(1); }} className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
          <option value="">Read/unread</option>
          <option value="false">Unread</option>
          <option value="true">Read</option>
        </select>
      </div>

      <div className="glass rounded-2xl divide-y divide-border">
        {(loading || searchLoading) && visibleNotifications.length === 0 && (
          <div className="p-5 text-sm text-muted-foreground">Loading notifications...</div>
        )}

        {!loading && !searchLoading && visibleNotifications.length === 0 && (
          <div className="p-5 text-sm text-muted-foreground">No notifications yet.</div>
        )}

        {visibleNotifications.map((notification) => {
          const cfg = iconMap[notification.type as keyof typeof iconMap] ?? iconMap.default;
          const Icon = cfg.Icon;
          return (
            <div
              key={notification.id}
              className={`flex gap-4 p-5 hover:bg-muted/20 transition ${
                notification.isRead ? "opacity-70" : "bg-primary/5"
              }`}
            >
              <div className={`size-10 shrink-0 rounded-xl flex items-center justify-center ${cfg.cls}`}>
                <Icon className="size-5" />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-baseline justify-between gap-2">
                  <div className="font-medium">{notification.title}</div>
                  <div className="text-xs text-muted-foreground shrink-0">
                    {new Date(notification.createdAt).toLocaleString()}
                  </div>
                </div>
                <div className="text-sm text-muted-foreground mt-0.5">{notification.message}</div>
              </div>
              {!notification.isRead && (
                <button
                  onClick={() => markRead(notification.id).then(() => setItems((current) => current.map((item) => item.id === notification.id ? { ...item, isRead: true } : item))).catch(() => undefined)}
                  className="h-8 px-3 rounded-lg text-xs border border-border hover:bg-muted/40"
                >
                  Mark read
                </button>
              )}
            </div>
          );
        })}
      </div>
      <div className="mt-4 flex justify-end gap-2 text-sm">
        <button disabled={page <= 1} onClick={() => setPage(page - 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Previous</button>
        <span className="h-9 px-3 flex items-center text-muted-foreground">Page {page} of {totalPages}</span>
        <button disabled={page >= totalPages} onClick={() => setPage(page + 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Next</button>
      </div>
    </>
  );
}
