import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { useMemo, useState, type FormEvent } from "react";
import { ShieldCheck, ArrowRight, ArrowLeft } from "lucide-react";
import { useAuth } from "@/lib/auth";
import { apiFetch } from "@/lib/api";
import {
  calculateFinancials,
  initialFinancialProfileForm,
  toFinancialProfilePayload,
  validateFinancialProfile,
  type FinancialProfileErrors,
  type FinancialProfileForm,
} from "@/lib/financialProfile";

export const Route = createFileRoute("/signup")({
  component: SignupPage,
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

function SignupPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [step, setStep] = useState<1 | 2>(1);
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [profile, setProfile] = useState<FinancialProfileForm>(initialFinancialProfileForm);
  const [profileErrors, setProfileErrors] = useState<FinancialProfileErrors>({});
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const financials = useMemo(() => calculateFinancials(profile), [profile]);

  const updateProfile = (name: keyof FinancialProfileForm, value: string) => {
    setProfile((current) => ({ ...current, [name]: value }));
    setProfileErrors((current) => ({ ...current, [name]: undefined }));
  };

  const continueToFinancials = (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setStep(2);
  };

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    const errors = validateFinancialProfile(profile);
    if (Object.keys(errors).length > 0) {
      setProfileErrors(errors);
      setError("Please fix the highlighted applicant fields.");
      return;
    }

    setLoading(true);
    const registered = await register({ fullName, email, password, confirmPassword, phoneNumber });

    if (!registered.ok) {
      setLoading(false);
      setError(registered.error);
      return;
    }

    try {
      const res = await apiFetch("/client-profile", {
        method: "POST",
        body: JSON.stringify(toFinancialProfilePayload(profile)),
      });

      if (!res.ok) {
        const data = await res.json().catch(() => null);
        const validationMessage = data?.errors ? Object.values(data.errors).flat().join(" ") : null;
        throw new Error(validationMessage || data?.error || data?.title || "Failed to save applicant profile.");
      }

      navigate({ to: "/dashboard" });
    } catch (err: any) {
      setError(err.message || "Failed to save applicant profile.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-10 bg-muted/40">
      <div className="w-full max-w-3xl">
        <Link to="/" className="flex items-center gap-2 justify-center mb-8">
          <div className="size-10 rounded-xl gradient-hero flex items-center justify-center">
            <ShieldCheck className="size-5 text-white" />
          </div>
          <div className="text-lg font-semibold">CreditIQ</div>
        </Link>

        <div className="bg-card border border-border rounded-2xl p-7 shadow-(--shadow-card)">
          <h1 className="text-xl font-semibold">Create your account</h1>
          <p className="text-sm text-muted-foreground mt-1">
            {step === 1 ? "Start with account details." : "Add applicant data for real credit analysis."}
          </p>

          {step === 1 ? (
            <form onSubmit={continueToFinancials} className="mt-6 space-y-4 max-w-md">
              <TextField label="Full name" value={fullName} onChange={setFullName} placeholder="Your name" />
              <TextField label="Email" type="email" value={email} onChange={setEmail} placeholder="you@credit.com" />
              <TextField label="Phone number" type="tel" value={phoneNumber} onChange={setPhoneNumber} placeholder="+1 555 123 4567" />
              <TextField label="Password" type="password" value={password} onChange={setPassword} placeholder="Password" />
              <TextField label="Confirm password" type="password" value={confirmPassword} onChange={setConfirmPassword} placeholder="Confirm password" />

              {error && <ErrorMessage message={error} />}

              <button type="submit" className="w-full h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition flex items-center justify-center gap-2">
                Continue <ArrowRight className="size-4" />
              </button>
            </form>
          ) : (
            <form onSubmit={submit} className="mt-6 space-y-5">
              <div className="grid sm:grid-cols-2 gap-4">
                {personalFields.map((field) => (
                  <NumberField
                    key={field.name}
                    name={field.name}
                    label={field.label}
                    value={profile[field.name]}
                    placeholder={field.placeholder}
                    error={profileErrors[field.name]}
                    onChange={updateProfile}
                  />
                ))}
                <SelectField
                  label="Employment status"
                  value={profile.employmentStatus}
                  onChange={(value) => updateProfile("employmentStatus", value)}
                  options={["Employed", "Self-employed", "Unemployed", "Student", "Retired"]}
                />
                <SelectField
                  label="Education"
                  value={profile.education}
                  onChange={(value) => updateProfile("education", value)}
                  options={["High school", "Bachelor", "Master", "PhD"]}
                />
                <SelectField
                  label="Marital status"
                  value={profile.maritalStatus}
                  onChange={(value) => updateProfile("maritalStatus", value)}
                  options={["Single", "Married", "Divorced", "Widowed"]}
                />
              </div>

              <div className="grid sm:grid-cols-2 gap-4">
                {debtFields.map((field) => (
                  <NumberField
                    key={field.name}
                    name={field.name}
                    label={field.label}
                    value={profile[field.name]}
                    placeholder="0"
                    error={profileErrors[field.name]}
                    onChange={updateProfile}
                  />
                ))}
              </div>

              <DebtSummary financials={financials} />
              {error && <ErrorMessage message={error} />}

              <div className="flex flex-col sm:flex-row gap-3">
                <button type="button" onClick={() => setStep(1)} className="h-11 px-5 rounded-xl border border-border text-sm hover:bg-muted/40 inline-flex items-center justify-center gap-2">
                  <ArrowLeft className="size-4" /> Back
                </button>
                <button type="submit" disabled={loading} className="h-11 px-5 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition inline-flex items-center justify-center gap-2 disabled:opacity-60">
                  {loading ? "Creating account..." : "Create account"} <ArrowRight className="size-4" />
                </button>
              </div>
            </form>
          )}
        </div>

        <div className="text-center text-xs text-muted-foreground mt-6 flex flex-col gap-2">
          <Link to="/login" className="hover:text-foreground transition">Already have an account? Sign in</Link>
          <Link to="/" className="hover:text-foreground transition">Back to home</Link>
        </div>
      </div>
    </div>
  );
}

function TextField({ label, value, onChange, placeholder, type = "text" }: { label: string; value: string; onChange: (value: string) => void; placeholder: string; type?: string }) {
  return (
    <div>
      <label className="text-xs font-medium text-foreground">{label}</label>
      <input type={type} value={value} onChange={(e) => onChange(e.target.value)} placeholder={placeholder} className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40" required />
    </div>
  );
}

function NumberField({ name, label, value, placeholder, error, onChange }: { name: keyof FinancialProfileForm; label: string; value: string; placeholder: string; error?: string; onChange: (name: keyof FinancialProfileForm, value: string) => void }) {
  return (
    <div>
      <label className="text-xs font-medium text-foreground">{label}</label>
      <input type="number" value={value} min={name === "creditScore" ? 300 : 0} max={name === "creditScore" ? 850 : undefined} step="1" onChange={(e) => onChange(name, e.target.value)} placeholder={placeholder} className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40" required />
      {error && <div className="mt-1 text-xs text-destructive">{error}</div>}
    </div>
  );
}

function SelectField({ label, value, options, onChange }: { label: string; value: string; options: string[]; onChange: (value: string) => void }) {
  return (
    <div>
      <label className="text-xs font-medium text-foreground">{label}</label>
      <select value={value} onChange={(e) => onChange(e.target.value)} className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40">
        {options.map((option) => <option key={option}>{option}</option>)}
      </select>
    </div>
  );
}

function DebtSummary({ financials }: { financials: ReturnType<typeof calculateFinancials> }) {
  return (
    <div className="grid sm:grid-cols-3 gap-3 rounded-xl bg-muted/40 p-4 text-sm">
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

function ErrorMessage({ message }: { message: string }) {
  return (
    <div className="text-xs rounded-lg px-3 py-2 bg-destructive/10 text-destructive border border-destructive/20">
      {message}
    </div>
  );
}
