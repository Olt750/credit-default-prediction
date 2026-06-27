import { useEffect, useState } from "react";
import { apiFetch } from "@/lib/api";
import { RiskBadge, StatusBadge } from "./RiskBadge";

type PredictionRow = {
  id: string;
  client: string;
  amount: number;
  score: number;
  level: "Low" | "Medium" | "High";
  model: string;
  date: string;
  status: "Approved" | "Pending" | "Rejected";
};

export function PredictionTable({ limit, withAction = false }: { limit?: number; withAction?: boolean }) {
  const [rows, setRows] = useState<PredictionRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadPredictions() {
      setLoading(true);
      setError(null);

      try {
        const query = limit ? `?limit=${limit}` : "";
        const res = await apiFetch(`/dashboard/recent-predictions${query}`);

        if (!res.ok) {
          throw new Error("Failed to load recent predictions.");
        }

        const data = await res.json();
        const mapped = (Array.isArray(data) ? data : []).map((item: any) => ({
          id: String(item.id),
          client: item.client ?? "Current User",
          amount: Number(item.amount ?? 0),
          score: Number(item.score ?? 0),
          level: item.level ?? "Low",
          model: item.model ?? "Random Forest",
          date: item.date ? new Date(item.date).toLocaleDateString() : "",
          status: item.status ?? "Pending",
        }));

        setRows(mapped);
      } catch (err: any) {
        setError(err.message || "Failed to load recent predictions.");
      } finally {
        setLoading(false);
      }
    }

    loadPredictions();
  }, [limit]);

  if (loading) {
    return <div className="p-4 text-center text-muted-foreground">Loading predictions...</div>;
  }

  if (error) {
    return <div className="p-4 text-center text-destructive">{error}</div>;
  }

  if (!rows.length) {
    return <div className="p-4 text-center text-muted-foreground">No predictions available.</div>;
  }
  return (
    <div className="overflow-x-auto -mx-5">
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-xs uppercase tracking-wider text-muted-foreground border-b border-border">
            <th className="py-3 px-5 font-medium">ID</th>
            <th className="py-3 pr-4 font-medium">Client</th>
            <th className="py-3 pr-4 font-medium">Loan</th>
            <th className="py-3 pr-4 font-medium">Risk Score</th>
            <th className="py-3 pr-4 font-medium">Level</th>
            <th className="py-3 pr-4 font-medium">Model</th>
            <th className="py-3 pr-4 font-medium">Date</th>
            <th className="py-3 pr-4 font-medium">Status</th>
            {withAction && <th className="py-3 pr-5 font-medium">Action</th>}
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.id} className="border-b border-border/50 hover:bg-muted/30 transition">
              <td className="py-3 px-5 font-mono text-xs text-muted-foreground">{r.id}</td>
              <td className="py-3 pr-4 font-medium">{r.client}</td>
              <td className="py-3 pr-4">€{r.amount.toLocaleString()}</td>
              <td className="py-3 pr-4">
                <div className="flex items-center gap-2">
                  <div className="h-1.5 w-20 rounded-full bg-muted overflow-hidden">
                    <div
                      className="h-full rounded-full"
                      style={{
                        width: `${r.score}%`,
                        background: r.score > 65 ? "var(--destructive)" : r.score > 35 ? "var(--warning)" : "var(--success)",
                      }}
                    />
                  </div>
                  <span className="text-xs font-mono">{r.score}%</span>
                </div>
              </td>
              <td className="py-3 pr-4"><RiskBadge level={r.level} /></td>
              <td className="py-3 pr-4 text-muted-foreground">{r.model}</td>
              <td className="py-3 pr-4 text-muted-foreground">{r.date}</td>
              <td className="py-3 pr-4"><StatusBadge status={r.status} /></td>
              {withAction && (
                <td className="py-3 pr-5">
                  <button className="text-xs text-primary hover:underline">View details</button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
