import { createFileRoute } from "@tanstack/react-router";
import { Trophy, Info } from "lucide-react";
import { ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from "recharts";
import { PageHeader } from "@/components/app/PageHeader";
import { ChartCard } from "@/components/app/ChartCard";
import { modelMetrics, confusionMatrices } from "@/data/mockData";

export const Route = createFileRoute("/_app/models")({
  component: ModelsPage,
});

const tooltipStyle = {
  background: "var(--popover)",
  border: "1px solid var(--border)",
  borderRadius: 12,
  fontSize: 12,
  color: "var(--foreground)",
};

function ModelsPage() {
  const best = modelMetrics.reduce((a, b) => (b.f1 > a.f1 ? b : a));
  return (
    <>
      <PageHeader title="Models Comparison" subtitle="Side-by-side classifier benchmarks on the validation set." />

      <div className="grid lg:grid-cols-3 gap-4">
        <div className="glass rounded-2xl p-6 lg:col-span-2">
          <h3 className="font-semibold mb-4">Metrics</h3>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wider text-muted-foreground border-b border-border">
                  <th className="py-3 pr-4 font-medium">Model</th>
                  <th className="py-3 pr-4 font-medium">Accuracy</th>
                  <th className="py-3 pr-4 font-medium">Precision</th>
                  <th className="py-3 pr-4 font-medium">Recall</th>
                  <th className="py-3 pr-4 font-medium">F1-Score</th>
                </tr>
              </thead>
              <tbody>
                {modelMetrics.map((m) => (
                  <tr key={m.model} className="border-b border-border/50">
                    <td className="py-3 pr-4 font-medium">{m.model}</td>
                    <td className="py-3 pr-4 font-mono">{(m.accuracy * 100).toFixed(1)}%</td>
                    <td className="py-3 pr-4 font-mono">{(m.precision * 100).toFixed(1)}%</td>
                    <td className="py-3 pr-4 font-mono">{(m.recall * 100).toFixed(1)}%</td>
                    <td className="py-3 pr-4 font-mono">{(m.f1 * 100).toFixed(1)}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="glass rounded-2xl p-6 relative overflow-hidden">
          <div className="absolute inset-0 -z-10 gradient-hero opacity-30" />
          <Trophy className="size-7 text-warning" />
          <div className="text-xs uppercase tracking-wider text-muted-foreground mt-4">Best model</div>
          <div className="text-2xl font-semibold mt-1">{best.model}</div>
          <div className="mt-4 space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-muted-foreground">F1</span><span className="font-mono">{(best.f1 * 100).toFixed(1)}%</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Accuracy</span><span className="font-mono">{(best.accuracy * 100).toFixed(1)}%</span></div>
            <div className="flex justify-between"><span className="text-muted-foreground">Recall</span><span className="font-mono">{(best.recall * 100).toFixed(1)}%</span></div>
          </div>
        </div>
      </div>

      <ChartCard title="Comparative performance" className="mt-4">
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={modelMetrics.map((m) => ({ ...m, accuracy: m.accuracy * 100, precision: m.precision * 100, recall: m.recall * 100, f1: m.f1 * 100 }))}>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
            <XAxis dataKey="model" stroke="var(--muted-foreground)" fontSize={11} />
            <YAxis stroke="var(--muted-foreground)" fontSize={11} domain={[60, 100]} />
            <Tooltip contentStyle={tooltipStyle} />
            <Legend wrapperStyle={{ fontSize: 12 }} />
            <Bar dataKey="accuracy" fill="var(--chart-1)" radius={[6, 6, 0, 0]} />
            <Bar dataKey="precision" fill="var(--chart-2)" radius={[6, 6, 0, 0]} />
            <Bar dataKey="recall" fill="var(--chart-3)" radius={[6, 6, 0, 0]} />
            <Bar dataKey="f1" fill="var(--chart-4)" radius={[6, 6, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </ChartCard>

      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4 mt-4">
        {confusionMatrices.map((c) => (
          <div key={c.model} className="glass rounded-2xl p-5">
            <div className="text-xs text-muted-foreground">{c.model}</div>
            <div className="text-sm font-semibold mt-1">Confusion matrix</div>
            <div className="mt-4 grid grid-cols-2 gap-1.5">
              <Cell label="TN" value={c.tn} tint="success" />
              <Cell label="FP" value={c.fp} tint="destructive" />
              <Cell label="FN" value={c.fn} tint="destructive" />
              <Cell label="TP" value={c.tp} tint="success" />
            </div>
          </div>
        ))}
      </div>

      <div className="glass rounded-2xl p-5 mt-4 flex gap-3 items-start">
        <Info className="size-5 text-primary shrink-0 mt-0.5" />
        <div className="text-sm text-muted-foreground">
          Hyperparameter tuning and feature selection are planned for the next iteration.
          Current scores reflect baseline configurations on the validation split.
        </div>
      </div>
    </>
  );
}

function Cell({ label, value, tint }: { label: string; value: number; tint: "success" | "destructive" }) {
  const cls = tint === "success" ? "bg-success/15 text-success" : "bg-destructive/15 text-destructive";
  return (
    <div className={`rounded-lg p-3 ${cls}`}>
      <div className="text-[10px] opacity-80">{label}</div>
      <div className="text-lg font-semibold font-mono">{value}</div>
    </div>
  );
}