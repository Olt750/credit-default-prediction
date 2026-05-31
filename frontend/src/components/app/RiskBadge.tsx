import type { RiskLevel } from "@/data/mockData";

export function RiskBadge({ level }: { level: RiskLevel | string }) {
  const map: Record<string, string> = {
    Low: "bg-success/15 text-success border-success/30",
    Medium: "bg-warning/15 text-warning border-warning/30",
    High: "bg-destructive/15 text-destructive border-destructive/30",
  };
  return (
    <span className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium border ${map[level] ?? "bg-muted text-muted-foreground border-border"}`}>
      <span className="size-1.5 rounded-full bg-current" />
      {level} Risk
    </span>
  );
}

export function StatusBadge({ status }: { status: string }) {
  const map: Record<string, string> = {
    Approved: "bg-success/15 text-success border-success/30",
    Pending: "bg-warning/15 text-warning border-warning/30",
    Rejected: "bg-destructive/15 text-destructive border-destructive/30",
  };
  return (
    <span className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium border ${map[status] ?? "bg-muted text-muted-foreground border-border"}`}>
      {status}
    </span>
  );
}