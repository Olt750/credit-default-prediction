import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { X } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { RiskBadge, StatusBadge } from "@/components/app/RiskBadge";
import { searchList } from "@/services/additionalFeaturesApi";

export const Route = createFileRoute("/_app/predictions")({
  component: PredictionsPage,
});

type Prediction = {
  id: string;
  amount: number;
  score: number;
  level: string;
  date: string;
  explanation: string;
  status: string;
  model: string;
  client: string;
};

function PredictionsPage() {
  const [open, setOpen] = useState<string | null>(null);
  const [predictions, setPredictions] = useState<Prediction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [keyword, setKeyword] = useState("");
  const [riskLevel, setRiskLevel] = useState("");
  const [minRiskScore, setMinRiskScore] = useState("");
  const [maxRiskScore, setMaxRiskScore] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [sortDirection, setSortDirection] = useState("desc");

  useEffect(() => {
    async function fetchPredictions() {
      setLoading(true);
      setError(null);

      try {
        const data = await searchList<any>("predictions", {
          keyword,
          riskLevel,
          minRiskScore,
          maxRiskScore,
          page,
          pageSize: 10,
          sortBy: "createdAt",
          sortDirection,
        });
        const arr = data.items ?? [];

        const mapped: Prediction[] = arr.map((p: any) => ({
          id: String(p.id),
          amount: p.loanAmount ?? p.amount ?? 0,
          score: p.riskScore ?? p.score ?? 0,
          level: p.riskLevel ?? p.level ?? "Low",
          date: p.createdAt ? new Date(p.createdAt).toLocaleDateString() : p.date ?? "",
          explanation: p.explanationMessage ?? p.explanation ?? "No explanation available.",
          status: p.status ?? statusFromRisk(p.riskLevel ?? p.level ?? "Medium"),
          model: p.model ?? "Random Forest",
          client: p.client ?? p.user?.fullName ?? p.fullName ?? "Current User",
        }));

        setPredictions(mapped);
        setTotalPages(data.totalPages || 1);
      } catch (err: any) {
        setError(err.message || "Failed to load predictions");
      } finally {
        setLoading(false);
      }
    }

    fetchPredictions();
  }, [keyword, riskLevel, minRiskScore, maxRiskScore, page, sortDirection]);

  const detail = predictions.find((p) => p.id === open) ?? predictions[0];

  if (loading) {
    return <div className="p-10 text-center">Loading predictions...</div>;
  }

  if (error) {
    return <div className="p-10 text-center text-destructive">{error}</div>;
  }

  return (
    <>
      <PageHeader
        title="Predictions"
        subtitle="All loan prediction records produced by the model registry."
      />

      <div className="glass rounded-2xl p-4 mb-4 flex flex-wrap gap-3">
        <input value={keyword} onChange={(e) => { setKeyword(e.target.value); setPage(1); }} placeholder="Search predictions" className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm flex-1 min-w-56" />
        <select value={riskLevel} onChange={(e) => { setRiskLevel(e.target.value); setPage(1); }} className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
          <option value="">All risk</option>
          <option>Low</option>
          <option>Medium</option>
          <option>High</option>
        </select>
        <input value={minRiskScore} onChange={(e) => setMinRiskScore(e.target.value)} type="number" placeholder="Min score" className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm w-28" />
        <input value={maxRiskScore} onChange={(e) => setMaxRiskScore(e.target.value)} type="number" placeholder="Max score" className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm w-28" />
        <button onClick={() => setSortDirection(sortDirection === "asc" ? "desc" : "asc")} className="h-10 px-3 rounded-lg border border-border text-sm">
          {sortDirection === "asc" ? "Oldest" : "Newest"}
        </button>
      </div>

      <div className="glass rounded-2xl p-5">
        <div
          onClick={(e) => {
            const tr = (e.target as HTMLElement).closest("tr");
            if (tr?.dataset.id) setOpen(tr.dataset.id);
          }}
        >
          <ClickableTable predictions={predictions} />
        </div>
      </div>

      {open && detail && (
        <div
          className="fixed inset-0 z-40 flex items-center justify-center p-4 bg-background/70 backdrop-blur-sm"
          onClick={() => setOpen(null)}
        >
          <div
            className="glass rounded-2xl max-w-lg w-full p-6"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-start justify-between">
              <div>
                <div className="text-xs text-muted-foreground font-mono">{detail.id}</div>
                <h3 className="text-xl font-semibold mt-1">{detail.client}</h3>
              </div>
              <button
                onClick={() => setOpen(null)}
                className="size-8 rounded-lg hover:bg-muted flex items-center justify-center"
              >
                <X className="size-4" />
              </button>
            </div>

            <div className="mt-5 grid grid-cols-2 gap-3">
              <div className="rounded-xl bg-muted/40 p-3">
                <div className="text-xs text-muted-foreground">Loan amount</div>
                <div className="font-semibold mt-1">
                  €{detail.amount?.toLocaleString?.() ?? detail.amount}
                </div>
              </div>

              <div className="rounded-xl bg-muted/40 p-3">
                <div className="text-xs text-muted-foreground">Risk score</div>
                <div className="font-semibold mt-1">{detail.score}%</div>
              </div>

              <div className="rounded-xl bg-muted/40 p-3 flex items-center justify-between">
                <span className="text-xs text-muted-foreground">Risk level</span>
                <RiskBadge level={detail.level} />
              </div>

              <div className="rounded-xl bg-muted/40 p-3 flex items-center justify-between">
                <span className="text-xs text-muted-foreground">Status</span>
                <StatusBadge status={detail.status} />
              </div>
            </div>

            <div className="mt-5 rounded-xl bg-muted/40 p-4 text-sm">
              <div className="text-xs text-muted-foreground mb-1">Model & explanation</div>
              <div className="font-medium">{detail.model}</div>
              <p className="text-xs text-muted-foreground mt-2 leading-relaxed">
                {detail.explanation}
              </p>
            </div>
          </div>
        </div>
      )}
      <Pagination page={page} totalPages={totalPages} onPage={setPage} />
    </>
  );
}

function statusFromRisk(level: string) {
  if (level === "Low") return "Approved";
  if (level === "High") return "Rejected";
  return "Pending";
}

function ClickableTable({ predictions }: { predictions: Prediction[] }) {
  return (
    <div className="overflow-x-auto -mx-5">
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-xs uppercase tracking-wider text-muted-foreground border-b border-border">
            <th className="py-3 px-5 font-medium">ID</th>
            <th className="py-3 pr-4 font-medium">Client</th>
            <th className="py-3 pr-4 font-medium">Loan</th>
            <th className="py-3 pr-4 font-medium">Score</th>
            <th className="py-3 pr-4 font-medium">Level</th>
            <th className="py-3 pr-4 font-medium">Model</th>
            <th className="py-3 pr-4 font-medium">Date</th>
            <th className="py-3 pr-4 font-medium">Status</th>
            <th className="py-3 pr-5 font-medium">Action</th>
          </tr>
        </thead>

        <tbody>
          {!predictions.length && (
            <tr>
              <td colSpan={9} className="py-6 px-5 text-center text-muted-foreground">No predictions found.</td>
            </tr>
          )}
          {predictions.map((r) => (
            <tr
              key={r.id}
              data-id={r.id}
              className="border-b border-border/50 hover:bg-muted/30 cursor-pointer transition"
            >
              <td className="py-3 px-5 font-mono text-xs text-muted-foreground">{r.id}</td>
              <td className="py-3 pr-4 font-medium">{r.client}</td>
              <td className="py-3 pr-4">€{r.amount?.toLocaleString?.() ?? r.amount}</td>
              <td className="py-3 pr-4 font-mono">{r.score}%</td>
              <td className="py-3 pr-4">
                <RiskBadge level={r.level} />
              </td>
              <td className="py-3 pr-4 text-muted-foreground">{r.model}</td>
              <td className="py-3 pr-4 text-muted-foreground">{r.date}</td>
              <td className="py-3 pr-4">
                <StatusBadge status={r.status} />
              </td>
              <td className="py-3 pr-5 text-xs text-primary">View details</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Pagination({ page, totalPages, onPage }: { page: number; totalPages: number; onPage: (page: number) => void }) {
  return (
    <div className="mt-4 flex justify-end gap-2 text-sm">
      <button disabled={page <= 1} onClick={() => onPage(page - 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Previous</button>
      <span className="h-9 px-3 flex items-center text-muted-foreground">Page {page} of {totalPages}</span>
      <button disabled={page >= totalPages} onClick={() => onPage(page + 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Next</button>
    </div>
  );
}
