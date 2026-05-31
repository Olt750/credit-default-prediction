import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { X } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { RiskBadge, StatusBadge } from "@/components/app/RiskBadge";
import { recentPredictions } from "@/data/mockData";

export const Route = createFileRoute("/_app/predictions")({
  component: PredictionsPage,
});

function PredictionsPage() {
  const [open, setOpen] = useState<string | null>(null);
  const detail = recentPredictions.find((p) => p.id === open) ?? recentPredictions[0];

  return (
    <>
      <PageHeader title="Predictions" subtitle="All loan prediction records produced by the model registry." />

      <div className="glass rounded-2xl p-4 mb-4 flex flex-wrap gap-3">
        {[
          { label: "Risk Level", options: ["All", "Low", "Medium", "High"] },
          { label: "Model", options: ["All", "Logistic Regression", "Decision Tree", "Random Forest", "Neural Network"] },
          { label: "Status", options: ["All", "Approved", "Pending", "Rejected"] },
          { label: "Date Range", options: ["Last 7 days", "Last 30 days", "Last 90 days", "All time"] },
        ].map((f) => (
          <div key={f.label} className="flex-1 min-w-[160px]">
            <label className="text-[10px] uppercase tracking-wider text-muted-foreground">{f.label}</label>
            <select className="mt-1 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
              {f.options.map((o) => <option key={o}>{o}</option>)}
            </select>
          </div>
        ))}
      </div>

      <div className="glass rounded-2xl p-5">
        <div onClick={(e) => {
          const tr = (e.target as HTMLElement).closest("tr");
          if (tr?.dataset.id) setOpen(tr.dataset.id);
        }}>
          <ClickableTable />
        </div>
      </div>

      {open && (
        <div className="fixed inset-0 z-40 flex items-center justify-center p-4 bg-background/70 backdrop-blur-sm" onClick={() => setOpen(null)}>
          <div className="glass rounded-2xl max-w-lg w-full p-6" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-start justify-between">
              <div>
                <div className="text-xs text-muted-foreground font-mono">{detail.id}</div>
                <h3 className="text-xl font-semibold mt-1">{detail.client}</h3>
              </div>
              <button onClick={() => setOpen(null)} className="size-8 rounded-lg hover:bg-muted flex items-center justify-center">
                <X className="size-4" />
              </button>
            </div>
            <div className="mt-5 grid grid-cols-2 gap-3">
              <div className="rounded-xl bg-muted/40 p-3">
                <div className="text-xs text-muted-foreground">Loan amount</div>
                <div className="font-semibold mt-1">€{detail.amount.toLocaleString()}</div>
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
                The applicant's credit history, debt-to-income ratio, and loan amount relative
                to income contributed most to this prediction. The model is 91% confident in
                this outcome.
              </p>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function ClickableTable() {
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
          {recentPredictions.map((r) => (
            <tr key={r.id} data-id={r.id} className="border-b border-border/50 hover:bg-muted/30 cursor-pointer transition">
              <td className="py-3 px-5 font-mono text-xs text-muted-foreground">{r.id}</td>
              <td className="py-3 pr-4 font-medium">{r.client}</td>
              <td className="py-3 pr-4">€{r.amount.toLocaleString()}</td>
              <td className="py-3 pr-4 font-mono">{r.score}%</td>
              <td className="py-3 pr-4"><RiskBadge level={r.level} /></td>
              <td className="py-3 pr-4 text-muted-foreground">{r.model}</td>
              <td className="py-3 pr-4 text-muted-foreground">{r.date}</td>
              <td className="py-3 pr-4"><StatusBadge status={r.status} /></td>
              <td className="py-3 pr-5 text-xs text-primary">View details</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}