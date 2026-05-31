import { createFileRoute } from "@tanstack/react-router";
import { Network, Cpu } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { nnArchitectures } from "@/data/mockData";

export const Route = createFileRoute("/_app/neural-network")({
  component: NNPage,
});

function NNPage() {
  return (
    <>
      <PageHeader title="Neural Network" subtitle="Compare candidate architectures considered for the final classifier." />

      <div className="grid lg:grid-cols-2 gap-4">
        {nnArchitectures.map((a, idx) => (
          <div key={a.name} className="glass rounded-2xl p-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <div className="size-9 rounded-xl gradient-hero flex items-center justify-center text-white">
                  <Network className="size-4" />
                </div>
                <div>
                  <div className="text-xs uppercase tracking-wider text-muted-foreground">Variant {idx + 1}</div>
                  <div className="font-semibold">{a.name}</div>
                </div>
              </div>
              <div className="text-right">
                <div className="text-xs text-muted-foreground">Accuracy</div>
                <div className="text-2xl font-semibold gradient-text">{(a.accuracy * 100).toFixed(1)}%</div>
              </div>
            </div>

            <div className="mt-6 space-y-2">
              {a.layers.map((l, i) => (
                <div key={i} className="flex items-center gap-3">
                  <div className="size-7 rounded-lg bg-muted/60 border border-border text-xs flex items-center justify-center font-mono">{i + 1}</div>
                  <div className="flex-1 rounded-lg bg-muted/40 border border-border px-3 py-2 text-sm">{l}</div>
                </div>
              ))}
            </div>

            <div className="mt-6 grid grid-cols-2 gap-3">
              <div className="rounded-xl bg-muted/40 p-3">
                <div className="text-xs text-muted-foreground">Loss</div>
                <div className="font-semibold mt-1 font-mono">{a.loss.toFixed(2)}</div>
              </div>
              <div className="rounded-xl bg-muted/40 p-3">
                <div className="text-xs text-muted-foreground">Params</div>
                <div className="font-semibold mt-1 font-mono">{idx === 0 ? "~1.2k" : "~3.4k"}</div>
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="glass rounded-2xl p-6 mt-4 flex gap-3 items-start">
        <Cpu className="size-5 text-accent shrink-0 mt-0.5" />
        <div className="text-sm text-muted-foreground">
          Architecture B wins on validation accuracy at the cost of a larger parameter count.
          Future iterations will explore dropout, batch normalization, and class-weight tuning.
        </div>
      </div>
    </>
  );
}