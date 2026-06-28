import { createFileRoute } from "@tanstack/react-router";
import { AlertTriangle, Info, CheckCircle2, TrendingUp } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { useNotifications } from "@/lib/notifications";

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

      <div className="glass rounded-2xl divide-y divide-border">
        {loading && notifications.length === 0 && (
          <div className="p-5 text-sm text-muted-foreground">Loading notifications...</div>
        )}

        {!loading && notifications.length === 0 && (
          <div className="p-5 text-sm text-muted-foreground">No notifications yet.</div>
        )}

        {notifications.map((notification) => {
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
                  onClick={() => markRead(notification.id).catch(() => undefined)}
                  className="h-8 px-3 rounded-lg text-xs border border-border hover:bg-muted/40"
                >
                  Mark read
                </button>
              )}
            </div>
          );
        })}
      </div>
    </>
  );
}
