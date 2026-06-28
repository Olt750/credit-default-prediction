import { createFileRoute } from "@tanstack/react-router";
import { ResponsiveContainer, ScatterChart, Scatter, XAxis, YAxis, CartesianGrid, Tooltip, ZAxis, LineChart, Line } from "recharts";
import { useEffect, useState } from "react";
import { PageHeader } from "@/components/app/PageHeader";
import { ChartCard } from "@/components/app/ChartCard";
import { Info } from "lucide-react";
import { ClusterPoint, ClusteringRow, getMlResult } from "@/services/mlResultsApi";

export const Route = createFileRoute("/_app/clustering")({
  component: ClusteringPage,
});

const clusterColors = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)", "var(--chart-4)", "var(--chart-5)"];

function ClusteringPage() {
  const [results, setResults] = useState<ClusteringRow[]>([]);
  const [points, setPoints] = useState<ClusterPoint[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    Promise.all([
      getMlResult<ClusteringRow[]>("clustering-results"),
      getMlResult<ClusterPoint[]>("clustering-points"),
    ])
      .then(([clusteringResults, clusterPoints]) => {
        if (!active) return;
        setResults(clusteringResults);
        setPoints(clusterPoints);
      })
      .catch((err) => active && setError(err instanceof Error ? err.message : "Unable to load clustering results."))
      .finally(() => active && setLoading(false));

    return () => {
      active = false;
    };
  }, []);

  const best = results.reduce<ClusteringRow | null>((winner, row) => {
    if (!winner) return row;
    return row.silhouette_score > winner.silhouette_score ? row : winner;
  }, null);
  const clusters = Array.from(new Set(points.map((p) => p.cluster))).sort((a, b) => a - b);

  return (
    <>
      <PageHeader title="Clustering Insights" subtitle="Real KMeans experiments with target labels removed before fitting." />

      {loading && <div className="glass rounded-2xl p-6 text-sm text-muted-foreground">Loading clustering results...</div>}
      {error && <div className="glass rounded-2xl p-6 text-sm text-destructive">{error}</div>}

      {!loading && !error && (
        <>
          <div className="glass rounded-2xl p-5 mb-4 flex gap-3 items-start">
            <Info className="size-5 text-primary shrink-0 mt-0.5" />
            <div className="text-sm text-muted-foreground">
              KMeans was tested with k = 2, 3, 4, and 5 after standardization. The target class was excluded during clustering and used only afterward for Adjusted Rand Index comparison.
              {best ? ` Best silhouette score in this run: k=${best.k}.` : ""}
            </div>
          </div>

          <div className="grid lg:grid-cols-3 gap-4">
            <ChartCard title="Cluster scatter plot" subtitle="PCA 2D projection" className="lg:col-span-2">
              <ResponsiveContainer width="100%" height={340}>
                <ScatterChart>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                  <XAxis type="number" dataKey="x" stroke="var(--muted-foreground)" fontSize={11} />
                  <YAxis type="number" dataKey="y" stroke="var(--muted-foreground)" fontSize={11} />
                  <ZAxis range={[55, 55]} />
                  <Tooltip cursor={{ strokeDasharray: "3 3" }} contentStyle={{
                    background: "var(--popover)", border: "1px solid var(--border)", borderRadius: 12, fontSize: 12,
                  }} />
                  {clusters.map((cluster) => (
                    <Scatter
                      key={cluster}
                      name={`Cluster ${cluster}`}
                      data={points.filter((p) => p.cluster === cluster)}
                      fill={clusterColors[cluster % clusterColors.length]}
                    />
                  ))}
                </ScatterChart>
              </ResponsiveContainer>
            </ChartCard>

            <div className="space-y-3">
              {results.map((row) => (
                <div key={row.k} className="glass rounded-2xl p-5">
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="text-xs uppercase tracking-wider text-muted-foreground">k = {row.k}</div>
                      <div className="text-2xl font-semibold mt-1">{row.silhouette_score.toFixed(3)}</div>
                    </div>
                    <div className="text-right text-xs text-muted-foreground">
                      <div>Inertia</div>
                      <div className="font-mono text-foreground">{row.inertia.toFixed(0)}</div>
                    </div>
                  </div>
                  <div className="mt-3 text-xs text-muted-foreground">ARI: <span className="font-mono text-foreground">{row.adjusted_rand_index.toFixed(3)}</span></div>
                </div>
              ))}
            </div>
          </div>

          <div className="grid lg:grid-cols-2 gap-4 mt-4">
            <ChartCard title="Elbow method" subtitle="Lower inertia is better, with diminishing returns">
              <ResponsiveContainer width="100%" height={260}>
                <LineChart data={results}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                  <XAxis dataKey="k" stroke="var(--muted-foreground)" fontSize={11} />
                  <YAxis stroke="var(--muted-foreground)" fontSize={11} />
                  <Tooltip />
                  <Line type="monotone" dataKey="inertia" stroke="var(--chart-1)" strokeWidth={2} />
                </LineChart>
              </ResponsiveContainer>
            </ChartCard>

            <ChartCard title="Silhouette score" subtitle="Higher means tighter and better separated clusters">
              <ResponsiveContainer width="100%" height={260}>
                <LineChart data={results}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                  <XAxis dataKey="k" stroke="var(--muted-foreground)" fontSize={11} />
                  <YAxis stroke="var(--muted-foreground)" fontSize={11} />
                  <Tooltip />
                  <Line type="monotone" dataKey="silhouette_score" stroke="var(--chart-3)" strokeWidth={2} />
                </LineChart>
              </ResponsiveContainer>
            </ChartCard>
          </div>
        </>
      )}
    </>
  );
}
