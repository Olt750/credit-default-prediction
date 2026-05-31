import { createFileRoute } from "@tanstack/react-router";
import { AlertTriangle, Info, CheckCircle2, TrendingUp } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { notifications } from "@/data/mockData";

export const Route = createFileRoute("/_app/notifications")({
  component: NotificationsPage,
});

const iconMap = {
  danger: { Icon: AlertTriangle, cls: "bg-destructive/15 text-destructive" },
  warning: { Icon: TrendingUp, cls: "bg-warning/15 text-warning" },
  success: { Icon: CheckCircle2, cls: "bg-success/15 text-success" },
  info: { Icon: Info, cls: "bg-primary/15 text-primary" },
} as const;

function NotificationsPage() {
  return (
    <>
      <PageHeader title="Notifications" subtitle="Real-time alerts from the prediction pipeline (mock stream)." />

      <div className="glass rounded-2xl divide-y divide-border">
        {notifications.map((n) => {
          const cfg = iconMap[n.type as keyof typeof iconMap];
          const Icon = cfg.Icon;
          return (
            <div key={n.id} className="flex gap-4 p-5 hover:bg-muted/20 transition">
              <div className={`size-10 shrink-0 rounded-xl flex items-center justify-center ${cfg.cls}`}>
                <Icon className="size-5" />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-baseline justify-between gap-2">
                  <div className="font-medium">{n.title}</div>
                  <div className="text-xs text-muted-foreground shrink-0">{n.time}</div>
                </div>
                <div className="text-sm text-muted-foreground mt-0.5">{n.desc}</div>
              </div>
            </div>
          );
        })}
      </div>
    </>
  );
}