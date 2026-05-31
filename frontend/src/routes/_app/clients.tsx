import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { Sparkles, AlertTriangle, Info } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";

export const Route = createFileRoute("/_app/clients")({
  component: ClientAnalysisPage,
});

const fields = [
  { name: "age", label: "Age", type: "number", placeholder: "34" },
  { name: "income", label: "Annual income (€)", type: "number", placeholder: "32000" },
  { name: "loanAmount", label: "Loan amount (€)", type: "number", placeholder: "12000" },
  { name: "creditScore", label: "Credit score", type: "number", placeholder: "640" },
  { name: "employment", label: "Employment status", type: "select", options: ["Employed", "Self-employed", "Unemployed", "Student", "Retired"] },
  { name: "loanTerm", label: "Loan term (months)", type: "number", placeholder: "36" },
  { name: "previousDefaults", label: "Previous defaults", type: "number", placeholder: "0" },
  { name: "dti", label: "Debt-to-income ratio", type: "number", placeholder: "0.32" },
  { name: "education", label: "Education", type: "select", options: ["High school", "Bachelor", "Master", "PhD"] },
  { name: "marital", label: "Marital status", type: "select", options: ["Single", "Married", "Divorced", "Widowed"] },
];

function ClientAnalysisPage() {
  const [result, setResult] = useState<null | { score: number }>(null);
  const [loading, setLoading] = useState(false);

  function analyze(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setResult(null);
    setTimeout(() => {
      setResult({ score: 78 });
      setLoading(false);
    }, 900);
  }

  return (
    <>
      <PageHeader
        title="Client Analysis"
        subtitle="Enter applicant data to generate a mock credit risk evaluation."
      />

      <div className="grid lg:grid-cols-3 gap-4">
        <form onSubmit={analyze} className="glass rounded-2xl p-6 lg:col-span-2">
          <div className="flex items-center gap-2 mb-5">
            <Sparkles className="size-4 text-primary" />
            <h3 className="font-semibold">Applicant profile</h3>
          </div>
          <div className="grid sm:grid-cols-2 gap-4">
            {fields.map((f) => (
              <div key={f.name}>
                <label className="text-xs text-muted-foreground">{f.label}</label>
                {f.type === "select" ? (
                  <select className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-ring/40">
                    {f.options!.map((o) => <option key={o}>{o}</option>)}
                  </select>
                ) : (
                  <input
                    type={f.type}
                    placeholder={f.placeholder}
                    className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/40"
                  />
                )}
              </div>
            ))}
          </div>
          <div className="mt-6 flex gap-3">
            <button type="submit" disabled={loading} className="h-11 px-5 rounded-xl gradient-hero text-sm font-medium text-white shadow-[var(--shadow-glow)] disabled:opacity-60">
              {loading ? "Analyzing…" : "Analyze Risk"}
            </button>
            <button type="reset" className="h-11 px-5 rounded-xl border border-border text-sm hover:bg-muted/40">
              Reset
            </button>
          </div>
        </form>

        <div className="glass rounded-2xl p-6">
          <div className="flex items-center gap-2 mb-2">
            <AlertTriangle className="size-4 text-warning" />
            <h3 className="font-semibold">Prediction result</h3>
          </div>
          {!result && !loading && (
            <div className="mt-8 text-center text-sm text-muted-foreground py-12">
              <Info className="size-8 mx-auto opacity-50" />
              <p className="mt-3">Submit applicant data to see the risk evaluation here.</p>
            </div>
          )}
          {loading && (
            <div className="mt-8 space-y-3">
              <div className="h-4 rounded bg-muted animate-pulse" />
              <div className="h-24 rounded bg-muted animate-pulse" />
              <div className="h-4 rounded bg-muted animate-pulse w-3/4" />
            </div>
          )}
          {result && (
            <div className="mt-4">
              <div className="flex items-end justify-between">
                <span className="text-xs text-muted-foreground">Risk Score</span>
                <span className="text-4xl font-semibold text-destructive">{result.score}%</span>
              </div>
              <div className="mt-3 h-2 rounded-full bg-muted overflow-hidden">
                <div className="h-full bg-gradient-to-r from-warning to-destructive" style={{ width: `${result.score}%` }} />
              </div>
              <dl className="mt-6 space-y-3 text-sm">
                <div className="flex justify-between border-b border-border/50 pb-2">
                  <dt className="text-muted-foreground">Risk Level</dt>
                  <dd className="font-medium text-destructive">High Risk</dd>
                </div>
                <div className="flex justify-between border-b border-border/50 pb-2">
                  <dt className="text-muted-foreground">Suggested Decision</dt>
                  <dd className="font-medium">Manual Review Required</dd>
                </div>
                <div className="flex justify-between border-b border-border/50 pb-2">
                  <dt className="text-muted-foreground">Model Used</dt>
                  <dd className="font-medium">Random Forest</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-muted-foreground">Confidence</dt>
                  <dd className="font-medium">0.91</dd>
                </div>
              </dl>
              <div className="mt-5 rounded-xl bg-muted/40 p-4 text-xs text-muted-foreground leading-relaxed">
                <span className="font-medium text-foreground">Explanation:</span> Income level,
                credit score, and previous default history were the strongest drivers of this
                prediction.
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}