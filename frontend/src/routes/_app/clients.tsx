import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useMemo, useState } from "react";
import { Sparkles, AlertTriangle, Info } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { RiskBadge } from "@/components/app/RiskBadge";
import { apiFetch } from "@/lib/api";
import { searchList } from "@/services/additionalFeaturesApi";
import {
  calculateFinancials,
  initialFinancialProfileForm,
  toFinancialProfilePayload,
  validateFinancialProfile,
  type FinancialProfileErrors,
  type FinancialProfileForm,
} from "@/lib/financialProfile";

export const Route = createFileRoute("/_app/clients")({
  component: ClientAnalysisPage,
});

const personalFields = [
  { name: "age", label: "Age", placeholder: "34" },
  { name: "annualIncome", label: "Annual income (EUR)", placeholder: "36000" },
  { name: "loanAmount", label: "Loan amount (EUR)", placeholder: "12000" },
  { name: "creditScore", label: "Credit score", placeholder: "640" },
  { name: "loanTermMonths", label: "Loan term (months)", placeholder: "36" },
  { name: "previousDefaults", label: "Previous defaults", placeholder: "0" },
] as const;

const debtFields = [
  { name: "monthlyCarLoanPayment", label: "Monthly car loan payment (EUR)" },
  { name: "monthlyMortgageOrRentPayment", label: "Monthly mortgage/rent payment (EUR)" },
  { name: "monthlyPersonalLoanPayment", label: "Monthly personal loan payment (EUR)" },
  { name: "monthlyCreditCardPayment", label: "Monthly credit card payment (EUR)" },
  { name: "monthlyOtherDebtPayment", label: "Monthly other debt payment (EUR)" },
] as const;

type PredictionResult = {
  riskLevel: "Low" | "Medium" | "High";
  riskScore: number;
  prediction: number;
  explanation: string;
};

type ClientProfileRow = {
  id: string;
  userName: string;
  userEmail: string;
  annualIncome: number;
  creditScore: number;
  employmentStatus: string;
  debtToIncomeRatio: number;
};

function ClientAnalysisPage() {
  const [form, setForm] = useState<FinancialProfileForm>(initialFinancialProfileForm);
  const [validationErrors, setValidationErrors] = useState<FinancialProfileErrors>({});
  const [result, setResult] = useState<PredictionResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [profileLoading, setProfileLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [clientRows, setClientRows] = useState<ClientProfileRow[]>([]);
  const [clientKeyword, setClientKeyword] = useState("");
  const [clientEmployment, setClientEmployment] = useState("");
  const [clientPage, setClientPage] = useState(1);
  const [clientTotalPages, setClientTotalPages] = useState(1);
  const financials = useMemo(() => calculateFinancials(form), [form]);

  useEffect(() => {
    async function loadProfile() {
      setProfileLoading(true);

      try {
        const res = await apiFetch("/client-profile/me");
        if (res.status === 404) return;
        if (!res.ok) throw new Error("Failed to load saved applicant profile.");

        const data = await res.json();
        setForm({
          age: String(data.age ?? ""),
          annualIncome: String(data.annualIncome ?? ""),
          loanAmount: String(data.loanAmount ?? ""),
          creditScore: String(data.creditScore ?? ""),
          employmentStatus: data.employmentStatus ?? "Employed",
          loanTermMonths: String(data.loanTermMonths ?? ""),
          previousDefaults: String(data.previousDefaults ?? "0"),
          education: data.education ?? "High school",
          maritalStatus: data.maritalStatus ?? "Single",
          monthlyCarLoanPayment: String(data.monthlyCarLoanPayment ?? "0"),
          monthlyMortgageOrRentPayment: String(data.monthlyMortgageOrRentPayment ?? "0"),
          monthlyPersonalLoanPayment: String(data.monthlyPersonalLoanPayment ?? "0"),
          monthlyCreditCardPayment: String(data.monthlyCreditCardPayment ?? "0"),
          monthlyOtherDebtPayment: String(data.monthlyOtherDebtPayment ?? "0"),
        });
      } catch (err: any) {
        setError(err.message || "Failed to load saved applicant profile.");
      } finally {
        setProfileLoading(false);
      }
    }

    loadProfile();
  }, []);

  useEffect(() => {
    async function loadClientRows() {
      const result = await searchList<ClientProfileRow>("client-profiles", {
        keyword: clientKeyword,
        employmentStatus: clientEmployment,
        page: clientPage,
        pageSize: 8,
        sortBy: "createdAt",
        sortDirection: "desc",
      });
      setClientRows(result.items);
      setClientTotalPages(result.totalPages || 1);
    }

    loadClientRows().catch(() => undefined);
  }, [clientKeyword, clientEmployment, clientPage]);

  async function analyze(e: React.FormEvent) {
    e.preventDefault();
    const errors = validateFinancialProfile(form);

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
      const payload = toFinancialProfilePayload(form);
      await saveProfile(payload);

      const res = await apiFetch("/predictions/predict", {
        method: "POST",
        body: JSON.stringify(payload),
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

  function updateField(name: keyof FinancialProfileForm, value: string) {
    setForm((current) => ({ ...current, [name]: value }));
    setValidationErrors((current) => ({ ...current, [name]: undefined }));
  }

  function resetForm() {
    setForm(initialFinancialProfileForm);
    setResult(null);
    setError(null);
    setValidationErrors({});
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

          {profileLoading ? (
            <div className="py-16 text-center text-sm text-muted-foreground">Loading applicant profile...</div>
          ) : (
            <>
              <div className="grid sm:grid-cols-2 gap-4">
                {personalFields.map((field) => (
                  <NumberField
                    key={field.name}
                    name={field.name}
                    label={field.label}
                    value={form[field.name]}
                    placeholder={field.placeholder}
                    error={validationErrors[field.name]}
                    onChange={updateField}
                  />
                ))}
                <SelectField
                  label="Employment status"
                  value={form.employmentStatus}
                  onChange={(value) => updateField("employmentStatus", value)}
                  options={["Employed", "Self-employed", "Unemployed", "Student", "Retired"]}
                />
                <SelectField
                  label="Education"
                  value={form.education}
                  onChange={(value) => updateField("education", value)}
                  options={["High school", "Bachelor", "Master", "PhD"]}
                />
                <SelectField
                  label="Marital status"
                  value={form.maritalStatus}
                  onChange={(value) => updateField("maritalStatus", value)}
                  options={["Single", "Married", "Divorced", "Widowed"]}
                />
              </div>

              <div className="grid sm:grid-cols-2 gap-4 mt-4">
                {debtFields.map((field) => (
                  <NumberField
                    key={field.name}
                    name={field.name}
                    label={field.label}
                    value={form[field.name]}
                    placeholder="0"
                    error={validationErrors[field.name]}
                    onChange={updateField}
                  />
                ))}
              </div>

              <DebtSummary financials={financials} />

              <div className="mt-6 flex gap-3">
                <button type="submit" disabled={loading} className="h-11 px-5 rounded-xl gradient-hero text-sm font-medium text-white shadow-[var(--shadow-glow)] disabled:opacity-60">
                  {loading ? "Analyzing..." : "Analyze Risk"}
                </button>
                <button type="reset" className="h-11 px-5 rounded-xl border border-border text-sm hover:bg-muted/40">
                  Reset
                </button>
              </div>
            </>
          )}

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
                  <dt className="text-muted-foreground">Debt-to-income ratio</dt>
                  <dd className="font-medium">{(financials.debtToIncomeRatio * 100).toFixed(2)}%</dd>
                </div>
              </dl>
              <div className="mt-5 rounded-xl bg-muted/40 p-4 text-xs text-muted-foreground leading-relaxed">
                <span className="font-medium text-foreground">Explanation:</span> {result.explanation}
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="mt-4 bg-card border border-border rounded-2xl shadow-[var(--shadow-card)] overflow-hidden">
        <div className="p-4 border-b border-border flex flex-wrap gap-3 items-center">
          <div className="font-semibold mr-auto">Client profiles</div>
          <input value={clientKeyword} onChange={(e) => { setClientKeyword(e.target.value); setClientPage(1); }} placeholder="Search name, email, status" className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm min-w-56" />
          <select value={clientEmployment} onChange={(e) => { setClientEmployment(e.target.value); setClientPage(1); }} className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
            <option value="">All employment</option>
            <option>Employed</option>
            <option>Self-employed</option>
            <option>Unemployed</option>
            <option>Student</option>
            <option>Retired</option>
          </select>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-xs uppercase tracking-wider text-muted-foreground border-b border-border bg-muted/40">
                <th className="py-3 px-5 font-medium">Client</th>
                <th className="py-3 pr-4 font-medium">Income</th>
                <th className="py-3 pr-4 font-medium">Credit Score</th>
                <th className="py-3 pr-4 font-medium">Employment</th>
                <th className="py-3 pr-5 font-medium">DTI</th>
              </tr>
            </thead>
            <tbody>
              {clientRows.map((row) => (
                <tr key={row.id} className="border-b border-border last:border-0 hover:bg-muted/30">
                  <td className="py-3 px-5"><div className="font-medium">{row.userName || row.userEmail}</div><div className="text-xs text-muted-foreground">{row.userEmail}</div></td>
                  <td className="py-3 pr-4">EUR {Number(row.annualIncome || 0).toLocaleString()}</td>
                  <td className="py-3 pr-4">{row.creditScore}</td>
                  <td className="py-3 pr-4">{row.employmentStatus}</td>
                  <td className="py-3 pr-5">{(Number(row.debtToIncomeRatio || 0) * 100).toFixed(2)}%</td>
                </tr>
              ))}
              {!clientRows.length && <tr><td colSpan={5} className="py-6 px-5 text-center text-muted-foreground">No client profiles found.</td></tr>}
            </tbody>
          </table>
        </div>
        <div className="p-4 flex justify-end gap-2 text-sm">
          <button disabled={clientPage <= 1} onClick={() => setClientPage(clientPage - 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Previous</button>
          <span className="h-9 px-3 flex items-center text-muted-foreground">Page {clientPage} of {clientTotalPages}</span>
          <button disabled={clientPage >= clientTotalPages} onClick={() => setClientPage(clientPage + 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Next</button>
        </div>
      </div>
    </>
  );
}

async function saveProfile(payload: ReturnType<typeof toFinancialProfilePayload>) {
  const existing = await apiFetch("/client-profile/me");
  const res = await apiFetch(existing.status === 404 ? "/client-profile" : "/client-profile/me", {
    method: existing.status === 404 ? "POST" : "PUT",
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const data = await res.json().catch(() => null);
    throw new Error(data?.title || data?.error || "Failed to save applicant profile.");
  }
}

function NumberField({ name, label, value, placeholder, error, onChange }: { name: keyof FinancialProfileForm; label: string; value: string; placeholder: string; error?: string; onChange: (name: keyof FinancialProfileForm, value: string) => void }) {
  return (
    <div>
      <label className="text-xs text-muted-foreground">{label}</label>
      <input
        type="number"
        placeholder={placeholder}
        value={value}
        required
        min={name === "creditScore" ? 300 : 0}
        max={name === "creditScore" ? 850 : undefined}
        step="1"
        onChange={(e) => onChange(name, e.target.value)}
        className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/40"
      />
      {error && <div className="mt-1 text-xs text-destructive">{error}</div>}
    </div>
  );
}

function SelectField({ label, value, options, onChange }: { label: string; value: string; options: string[]; onChange: (value: string) => void }) {
  return (
    <div>
      <label className="text-xs text-muted-foreground">{label}</label>
      <select value={value} onChange={(e) => onChange(e.target.value)} className="mt-1.5 w-full h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm focus:outline-none focus:ring-2 focus:ring-ring/40">
        {options.map((option) => <option key={option}>{option}</option>)}
      </select>
    </div>
  );
}

function DebtSummary({ financials }: { financials: ReturnType<typeof calculateFinancials> }) {
  return (
    <div className="mt-5 grid sm:grid-cols-3 gap-3 rounded-xl bg-muted/40 p-4 text-sm">
      <SummaryItem label="Monthly income" value={`EUR ${financials.monthlyIncome.toLocaleString(undefined, { maximumFractionDigits: 2 })}`} />
      <SummaryItem label="Total monthly debt" value={`EUR ${financials.totalMonthlyDebt.toLocaleString(undefined, { maximumFractionDigits: 2 })}`} />
      <SummaryItem label="Debt-to-income ratio" value={`${(financials.debtToIncomeRatio * 100).toFixed(2)}%`} />
    </div>
  );
}

function SummaryItem({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="font-semibold mt-1">{value}</div>
    </div>
  );
}
