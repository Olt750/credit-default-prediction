import type { LucideIcon } from "lucide-react";

export function StatCard({
  label,
  value,
  delta,
  trend = "up",
  icon: Icon,
  tint = "primary",
}: {
  label: string;
  value: string | number;
  delta?: string;
  trend?: "up" | "down";
  icon: LucideIcon;
  tint?: "primary" | "accent" | "success" | "warning" | "destructive";
}) {
  const tintClass = {
    primary: "bg-primary/10 text-primary",
    accent: "bg-accent/15 text-accent",
    success: "bg-success/10 text-success",
    warning: "bg-warning/15 text-warning",
    destructive: "bg-destructive/10 text-destructive",
  }[tint];

  return (
    <div className="bg-card border border-border rounded-2xl p-5 shadow-[var(--shadow-card)] hover:border-primary/30 transition-colors">
      <div className="flex items-start justify-between">
        <div>
          <div className="text-xs uppercase tracking-wider text-muted-foreground">{label}</div>
          <div className="text-2xl font-semibold mt-2 tracking-tight">{value}</div>
        </div>
        <div className={`size-10 rounded-xl ${tintClass} flex items-center justify-center`}>
          <Icon className="size-4.5" />
        </div>
      </div>
      {delta && (
        <div className="mt-3 text-xs flex items-center gap-1.5">
          <span className={trend === "up" ? "text-success" : "text-destructive"}>
            {trend === "up" ? "▲" : "▼"} {delta}
          </span>
          <span className="text-muted-foreground">vs last month</span>
        </div>
      )}
    </div>
  );
}