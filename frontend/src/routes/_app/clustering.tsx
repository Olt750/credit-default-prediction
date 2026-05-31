import { createFileRoute } from "@tanstack/react-router";
import { ResponsiveContainer, ScatterChart, Scatter, XAxis, YAxis, CartesianGrid, Tooltip, ZAxis } from "recharts";
import { PageHeader } from "@/components/app/PageHeader";
import { ChartCard } from "@/components/app/ChartCard";
import { clusters, clusterPoints } from "@/data/mockData";
import { Info } from "lucide-react";

export const Route = createFileRoute("/_app/clustering")({
  component: ClusteringPage,
});

const colorMap: Record<string, string> = {
  Low: "var(--success)",
  Medium: "var(--warning)",
  High: "var(--destructive)",
};

function ClusteringPage() {
  return (
    <>
      <PageHeader title="Clustering Insights" subtitle="Unsupervised view of the portfolio — labels removed before training." />

      <div className="glass rounded-2xl p-5 mb-4 flex gap-3 items-start">
        <Info className="size-5 text-primary shrink-0 mt-0.5" />
        <div className="text-sm text-muted-foreground">
          K-Means with k=3 was applied to applicant features after standardization. Risk
          labels were removed beforehand, then mapped post-hoc to interpret each cluster.
        </div>
      </div>

      <div className="grid lg:grid-cols-3 gap-4">
        <ChartCard title="Cluster scatter plot" subtitle="PCA 2D projection" className="lg:col-span-2">
          <ResponsiveContainer width="100%" height={340}>
            <ScatterChart>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis type="number" dataKey="x" stroke="var(--muted-foreground)" fontSize={11} />
              <YAxis type="number" dataKey="y" stroke="var(--muted-foreground)" fontSize={11} />
              <ZAxis range={[60, 60]} />
              <Tooltip cursor={{ strokeDasharray: "3 3" }} contentStyle={{
                background: "var(--popover)", border: "1px solid var(--border)", borderRadius: 12, fontSize: 12,
              }} />
              {["Low", "Medium", "High"].map((c) => (
                <Scatter key={c} name={c} data={clusterPoints.filter((p) => p.cluster === c)} fill={colorMap[c]} />
              ))}
            </ScatterChart>
          </ResponsiveContainer>
        </ChartCard>

        <div className="space-y-3">
          {clusters.map((c) => (
            <div key={c.name} className="glass rounded-2xl p-5">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-xs uppercase tracking-wider text-muted-foreground">{c.name}</div>
                  <div className="text-2xl font-semibold mt-1">{c.size.toLocaleString()}</div>
                </div>
                <div className="size-10 rounded-xl flex items-center justify-center" style={{ background: `color-mix(in oklab, ${c.color} 25%, transparent)` }}>
                  <div className="size-3 rounded-full" style={{ background: c.color }} />
                </div>
              </div>
              <div className="mt-3 text-xs text-muted-foreground">Avg. risk score: <span className="font-mono text-foreground">{c.avgScore}%</span></div>
            </div>
          ))}
        </div>
      </div>
    </>
  );
}