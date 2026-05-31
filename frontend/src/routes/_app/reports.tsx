import { createFileRoute } from "@tanstack/react-router";
import { FileText, Download, FileSpreadsheet, Sparkles } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { PredictionTable } from "@/components/app/PredictionTable";

export const Route = createFileRoute("/_app/reports")({
  component: ReportsPage,
});

function ReportsPage() {
  return (
    <>
      <PageHeader title="Reports" subtitle="Generate, preview, and export portfolio-level reports." />

      <div className="grid lg:grid-cols-3 gap-4">
        <div className="glass rounded-2xl p-6 lg:col-span-1 space-y-4">
          {[
            { label: "Date range", options: ["Last 7 days", "Last 30 days", "Last quarter", "Year to date"] },
            { label: "Risk level", options: ["All", "Low", "Medium", "High"] },
            { label: "Model", options: ["All", "Logistic Regression", "Decision Tree", "Random Forest", "Neural Network"] },
            { label: "Export format", options: ["CSV", "PDF", "XLSX", "JSON"] },
          ].map((f) => (
            <div key={f.label}>
              <label className="text-xs text-muted-foreground">{f.label}</label>
              <select className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
                {f.options.map((o) => <option key={o}>{o}</option>)}
              </select>
            </div>
          ))}
          <button className="w-full h-11 rounded-xl gradient-hero text-sm font-medium text-white shadow-[var(--shadow-glow)] flex items-center justify-center gap-2">
            <Sparkles className="size-4" /> Generate Report
          </button>
          <div className="grid grid-cols-2 gap-2">
            <button className="h-10 rounded-xl border border-border text-sm hover:bg-muted/40 flex items-center justify-center gap-1.5">
              <FileSpreadsheet className="size-4" /> CSV
            </button>
            <button className="h-10 rounded-xl border border-border text-sm hover:bg-muted/40 flex items-center justify-center gap-1.5">
              <Download className="size-4" /> PDF
            </button>
          </div>
        </div>

        <div className="glass rounded-2xl p-6 lg:col-span-2">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <FileText className="size-4 text-primary" />
              <h3 className="font-semibold">Report preview</h3>
            </div>
            <span className="text-xs text-muted-foreground">Last 30 days · All risk levels</span>
          </div>
          <div className="grid grid-cols-3 gap-3 mb-5">
            <div className="rounded-xl bg-muted/40 p-3"><div className="text-xs text-muted-foreground">Total</div><div className="text-xl font-semibold mt-1">812</div></div>
            <div className="rounded-xl bg-muted/40 p-3"><div className="text-xs text-muted-foreground">High risk</div><div className="text-xl font-semibold mt-1 text-destructive">194</div></div>
            <div className="rounded-xl bg-muted/40 p-3"><div className="text-xs text-muted-foreground">Approved</div><div className="text-xl font-semibold mt-1 text-success">521</div></div>
          </div>
          <PredictionTable limit={6} />
        </div>
      </div>
    </>
  );
}