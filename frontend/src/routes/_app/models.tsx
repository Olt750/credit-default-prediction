import { createFileRoute } from "@tanstack/react-router";
import { Trophy, Info } from "lucide-react";
import { ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from "recharts";
import { useEffect, useState } from "react";
import { PageHeader } from "@/components/app/PageHeader";
import { ChartCard } from "@/components/app/ChartCard";
import {
  FeatureImportanceRow,
  ModelComparisonRow,
  ModelMetadata,
  getMlResult,
} from "@/services/mlResultsApi";

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

function percent(value: number) {
  return `${(value * 100).toFixed(1)}%`;
}

function parseJsonObject(value: string) {
  try {
    return JSON.parse(value);
  } catch {
    return {};
  }
}

function ModelsPage() {
  const [models, setModels] = useState<ModelComparisonRow[]>([]);
  const [importance, setImportance] = useState<FeatureImportanceRow[]>([]);
  const [metadata, setMetadata] = useState<ModelMetadata | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    Promise.all([
      getMlResult<ModelComparisonRow[]>("model-comparison"),
      getMlResult<FeatureImportanceRow[]>("feature-importance"),
      getMlResult<ModelMetadata>("model-metadata"),
    ])
      .then(([comparison, featureImportance, modelMetadata]) => {
        if (!active) return;
        setModels(comparison);
        setImportance(featureImportance.slice(0, 10));
        setMetadata(modelMetadata);
      })
      .catch((err) => active && setError(err instanceof Error ? err.message : "Unable to load ML results."))
      .finally(() => active && setLoading(false));

    return () => {
      active = false;
    };
  }, []);

  const best = models[0];
  const chartData = models.map((m) => ({
    model: m.model,
    accuracy: m.test_accuracy * 100,
    precision: m.test_precision * 100,
    recall: m.test_recall * 100,
    f1: m.test_f1 * 100,
    auc: m.roc_auc * 100,
  }));

  return (
    <>
      <PageHeader title="Models Comparison" subtitle="Real tuned classifier benchmarks from the German Credit dataset." />

      {loading && <div className="glass rounded-2xl p-6 text-sm text-muted-foreground">Loading model results...</div>}
      {error && <div className="glass rounded-2xl p-6 text-sm text-destructive">{error}</div>}

      {!loading && !error && best && (
        <>
          <div className="grid lg:grid-cols-3 gap-4">
            <div className="glass rounded-2xl p-6 lg:col-span-2">
              <h3 className="font-semibold mb-4">Tuned Model Metrics</h3>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-left text-xs uppercase tracking-wider text-muted-foreground border-b border-border">
                      <th className="py-3 pr-4 font-medium">Model</th>
                      <th className="py-3 pr-4 font-medium">CV F1</th>
                      <th className="py-3 pr-4 font-medium">Accuracy</th>
                      <th className="py-3 pr-4 font-medium">Precision</th>
                      <th className="py-3 pr-4 font-medium">Recall</th>
                      <th className="py-3 pr-4 font-medium">F1</th>
                      <th className="py-3 pr-4 font-medium">ROC-AUC</th>
                    </tr>
                  </thead>
                  <tbody>
                    {models.map((m) => (
                      <tr key={m.model} className="border-b border-border/50">
                        <td className="py-3 pr-4 font-medium">{m.model}</td>
                        <td className="py-3 pr-4 font-mono">{percent(m.cv_score)}</td>
                        <td className="py-3 pr-4 font-mono">{percent(m.test_accuracy)}</td>
                        <td className="py-3 pr-4 font-mono">{percent(m.test_precision)}</td>
                        <td className="py-3 pr-4 font-mono">{percent(m.test_recall)}</td>
                        <td className="py-3 pr-4 font-mono">{percent(m.test_f1)}</td>
                        <td className="py-3 pr-4 font-mono">{percent(m.roc_auc)}</td>
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
              <div className="text-2xl font-semibold mt-1">{metadata?.selected_model_name ?? best.model}</div>
              <div className="mt-4 space-y-2 text-sm">
                <div className="flex justify-between"><span className="text-muted-foreground">F1</span><span className="font-mono">{percent(best.test_f1)}</span></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Accuracy</span><span className="font-mono">{percent(best.test_accuracy)}</span></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Recall</span><span className="font-mono">{percent(best.test_recall)}</span></div>
                <div className="flex justify-between"><span className="text-muted-foreground">Features</span><span className="font-mono">{metadata?.feature_count ?? 20}</span></div>
              </div>
            </div>
          </div>

          <ChartCard title="Comparative performance" className="mt-4">
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis dataKey="model" stroke="var(--muted-foreground)" fontSize={11} />
                <YAxis stroke="var(--muted-foreground)" fontSize={11} domain={[0, 100]} />
                <Tooltip contentStyle={tooltipStyle} />
                <Legend wrapperStyle={{ fontSize: 12 }} />
                <Bar dataKey="accuracy" fill="var(--chart-1)" radius={[6, 6, 0, 0]} />
                <Bar dataKey="precision" fill="var(--chart-2)" radius={[6, 6, 0, 0]} />
                <Bar dataKey="recall" fill="var(--chart-3)" radius={[6, 6, 0, 0]} />
                <Bar dataKey="f1" fill="var(--chart-4)" radius={[6, 6, 0, 0]} />
                <Bar dataKey="auc" fill="var(--chart-5)" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>

          <div className="grid lg:grid-cols-2 gap-4 mt-4">
            <div className="glass rounded-2xl p-6">
              <h3 className="font-semibold mb-4">Best Hyperparameters</h3>
              <div className="space-y-2 text-sm">
                {Object.entries(parseJsonObject(best.best_hyperparameters)).map(([key, value]) => (
                  <div key={key} className="flex justify-between gap-3 border-b border-border/50 pb-2">
                    <span className="text-muted-foreground">{key}</span>
                    <span className="font-mono text-right">{String(value)}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="glass rounded-2xl p-6">
              <h3 className="font-semibold mb-4">Top Feature Importance</h3>
              <div className="space-y-2 text-sm">
                {importance.map((row) => (
                  <div key={row.feature} className="grid grid-cols-[90px_1fr_70px] gap-3 items-center">
                    <span className="font-mono">{row.feature}</span>
                    <span className="text-muted-foreground truncate">{row.description}</span>
                    <span className="font-mono text-right">{Number(row.importance).toFixed(3)}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
            {models.map((c) => (
              <div key={c.model} className="glass rounded-2xl p-5">
                <div className="text-xs text-muted-foreground">{c.model}</div>
                <div className="text-sm font-semibold mt-1">Confusion matrix</div>
                <div className="mt-4 grid grid-cols-2 gap-1.5">
                  <Cell label="TN" value={c.confusion_matrix.tn} tint="success" />
                  <Cell label="FP" value={c.confusion_matrix.fp} tint="destructive" />
                  <Cell label="FN" value={c.confusion_matrix.fn} tint="destructive" />
                  <Cell label="TP" value={c.confusion_matrix.tp} tint="success" />
                </div>
              </div>
            ))}
          </div>

          <div className="glass rounded-2xl p-5 mt-4 flex gap-3 items-start">
            <Info className="size-5 text-primary shrink-0 mt-0.5" />
            <div className="text-sm text-muted-foreground">
              Metrics come from `ml/results/model_comparison_results.json`, generated by GridSearchCV on a stratified split of the real German Credit dataset.
            </div>
          </div>
        </>
      )}
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
