import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { Sparkles, AlertTriangle, Info } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { RiskBadge } from "@/components/app/RiskBadge";
import { apiFetch } from "@/lib/api";

export const Route = createFileRoute("/_app/clients")({
  component: ClientAnalysisPage,
});

const fields = [
  { name: "age", label: "Age", type: "number", placeholder: "34" },
  { name: "annualIncome", label: "Annual income (EUR)", type: "number", placeholder: "32000" },
  { name: "loanAmount", label: "Loan amount (EUR)", type: "number", placeholder: "12000" },
  { name: "creditScore", label: "Credit score", type: "number", placeholder: "640" },
  { name: "employmentStatus", label: "Employment status", type: "select", options: ["Employed", "Self-employed", "Unemployed", "Student", "Retired"] },
  { name: "loanTerm", label: "Loan term (months)", type: "number", placeholder: "36" },
  { name: "previousDefaults", label: "Previous defaults", type: "number", placeholder: "0" },
  { name: "debtToIncomeRatio", label: "Debt-to-income ratio", type: "number", placeholder: "0.32" },
  { name: "education", label: "Education", type: "select", options: ["High school", "Bachelor", "Master", "PhD"] },
  { name: "maritalStatus", label: "Marital status", type: "select", options: ["Single", "Married", "Divorced", "Widowed"] },
];

type FormState = {
  age: string;
  annualIncome: string;
  loanAmount: string;
  creditScore: string;
  employmentStatus: string;
  loanTerm: string;
  previousDefaults: string;
  debtToIncomeRatio: string;
  education: string;
  maritalStatus: string;
};

type PredictionResult = {
  riskLevel: "Low" | "Medium" | "High";
  riskScore: number;
  prediction: number;
  explanation: string;
};

type ValidationErrors = Partial<Record<keyof FormState, string>>;

const initialForm: FormState = {
  age: "",
  annualIncome: "",
  loanAmount: "",
  creditScore: "",
  employmentStatus: "Employed",
  loanTerm: "",
  previousDefaults: "0",
  debtToIncomeRatio: "",
  education: "High school",
  maritalStatus: "Single",
};

function ClientAnalysisPage() {
  const [form, setForm] = useState<FormState>(initialForm);
  const [result, setResult] = useState<PredictionResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});

  async function analyze(e: React.FormEvent) {
    e.preventDefault();
    const errors = validateForm(form);

    if (Object.keys(errors).length > 0) {
      setValidationErrors(errors);
      setResult(null);
      setError("Please fix the highlighted applicant fields.");
      return;
    }

    setValidationErrors({});
    setLoading(true);
    setResult(null);
    setError(null);

    try {
      const res = await apiFetch("/predictions/predict", {
        method: "POST",
        body: JSON.stringify({
          age: Number(form.age),
          annualIncome: Number(form.annualIncome),
          loanAmount: Number(form.loanAmount),
          creditScore: Number(form.creditScore),
          employmentStatus: form.employmentStatus,
          loanTerm: Number(form.loanTerm),
          previousDefaults: Number(form.previousDefaults),
          debtToIncomeRatio: Number(form.debtToIncomeRatio),
          education: form.education,
          maritalStatus: form.maritalStatus,
        }),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => null);
        const validationMessage = data?.errors
          ? Object.values(data.errors).flat().join(" ")
          : null;
        throw new Error(validationMessage || data?.title || data?.error || "Prediction request failed.");
      }

      setResult(await res.json());
    } catch (err: any) {
      setError(err.message || "Backend unavailable. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  function updateField(name: keyof FormState, value: string) {
    setForm((current) => ({ ...current, [name]: value }));
    setValidationErrors((current) => ({ ...current, [name]: undefined }));
  }

  function resetForm() {
    setForm(initialForm);
    setResult(null);
    setError(null);
  }

  const scoreColor =
    result?.riskLevel === "Low"
      ? "text-success"
      : result?.riskLevel === "Medium"
        ? "text-warning"
        : "text-destructive";

  const barColor =
    result?.riskLevel === "Low"
      ? "bg-success"
      : result?.riskLevel === "Medium"
        ? "bg-warning"
        : "bg-destructive";

  return (
    <>
      <PageHeader
        title="Client Analysis"
        subtitle="Enter applicant data to generate a credit risk evaluation from the trained ML model."
      />

      <div className="grid lg:grid-cols-3 gap-4">
        <form onSubmit={analyze} onReset={resetForm} className="glass rounded-2xl p-6 lg:col-span-2">
          <div className="flex items-center gap-2 mb-5">
            <Sparkles className="size-4 text-primary" />
            <h3 className="font-semibold">Applicant profile</h3>
          </div>
          <div className="grid sm:grid-cols-2 gap-4">
            {fields.map((f) => {
              const fieldName = f.name as keyof FormState;

              return (
                <div key={f.name}>
                  <label className="text-xs text-muted-foreground">{f.label}</label>
                  {f.type === "select" ? (
                    <select
                      value={form[fieldName]}
                      onChange={(e) => updateField(fieldName, e.target.value)}
                      className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
                    >
                      {f.options!.map((o) => <option key={o}>{o}</option>)}
                    </select>
                  ) : (
                    <input
                      type={f.type}
                      placeholder={f.placeholder}
                      value={form[fieldName]}
                      required
                      min={f.name === "creditScore" ? 300 : f.name === "debtToIncomeRatio" || f.name === "previousDefaults" || f.name === "annualIncome" ? 0 : 1}
                      max={f.name === "creditScore" ? 850 : f.name === "debtToIncomeRatio" ? 1 : undefined}
                      step={f.name === "debtToIncomeRatio" ? "0.01" : "1"}
                      onChange={(e) => updateField(fieldName, e.target.value)}
                      className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/40"
                    />
                  )}
                  {validationErrors[fieldName] && (
                    <div className="mt-1 text-xs text-destructive">{validationErrors[fieldName]}</div>
                  )}
                </div>
              );
            })}
          </div>
          <div className="mt-6 flex gap-3">
            <button type="submit" disabled={loading} className="h-11 px-5 rounded-xl gradient-hero text-sm font-medium text-white shadow-[var(--shadow-glow)] disabled:opacity-60">
              {loading ? "Analyzing..." : "Analyze Risk"}
            </button>
            <button type="reset" className="h-11 px-5 rounded-xl border border-border text-sm hover:bg-muted/40">
              Reset
            </button>
          </div>
          {error && (
            <div className="mt-4 rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
              {error}
            </div>
          )}
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
                <span className={`text-4xl font-semibold ${scoreColor}`}>{result.riskScore}%</span>
              </div>
              <div className="mt-3 h-2 rounded-full bg-muted overflow-hidden">
                <div className={`h-full ${barColor}`} style={{ width: `${result.riskScore}%` }} />
              </div>
              <dl className="mt-6 space-y-3 text-sm">
                <div className="flex justify-between border-b border-border/50 pb-2">
                  <dt className="text-muted-foreground">Risk Level</dt>
                  <dd><RiskBadge level={result.riskLevel} /></dd>
                </div>
                <div className="flex justify-between border-b border-border/50 pb-2">
                  <dt className="text-muted-foreground">Prediction Status</dt>
                  <dd className="font-medium">{result.prediction === 0 ? "Default risk flagged" : "Good credit predicted"}</dd>
                </div>
                <div className="flex justify-between border-b border-border/50 pb-2">
                  <dt className="text-muted-foreground">Model Used</dt>
                  <dd className="font-medium">Random Forest</dd>
                </div>
              </dl>
              <div className="mt-5 rounded-xl bg-muted/40 p-4 text-xs text-muted-foreground leading-relaxed">
                <span className="font-medium text-foreground">Explanation:</span> {result.explanation}
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}

function validateForm(form: FormState) {
  const errors: ValidationErrors = {};
  const age = Number(form.age);
  const income = Number(form.annualIncome);
  const loanAmount = Number(form.loanAmount);
  const creditScore = Number(form.creditScore);
  const loanTerm = Number(form.loanTerm);
  const previousDefaults = Number(form.previousDefaults);
  const debtToIncomeRatio = Number(form.debtToIncomeRatio);

  if (!Number.isFinite(age) || age <= 0) errors.age = "Age must be greater than 0.";
  if (!Number.isFinite(income) || income < 0) errors.annualIncome = "Annual income cannot be negative.";
  if (!Number.isFinite(loanAmount) || loanAmount <= 0) errors.loanAmount = "Loan amount must be greater than 0.";
  if (!Number.isFinite(creditScore) || creditScore < 300 || creditScore > 850) errors.creditScore = "Credit score must be between 300 and 850.";
  if (!Number.isFinite(loanTerm) || loanTerm <= 0) errors.loanTerm = "Loan term must be greater than 0.";
  if (!Number.isFinite(previousDefaults) || previousDefaults < 0) errors.previousDefaults = "Previous defaults cannot be negative.";
  if (!Number.isFinite(debtToIncomeRatio) || debtToIncomeRatio < 0 || debtToIncomeRatio > 1) errors.debtToIncomeRatio = "Debt-to-income ratio must be between 0 and 1.";

  return errors;
}
