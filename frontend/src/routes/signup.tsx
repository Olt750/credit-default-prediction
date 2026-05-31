import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { useState, type FormEvent } from "react";
import { ShieldCheck, ArrowRight } from "lucide-react";
import { useAuth } from "@/lib/auth";

export const Route = createFileRoute("/signup")({
  component: SignupPage,
});

function SignupPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }
    setLoading(true);
    const res = await register({ fullName, email, password, confirmPassword, phoneNumber });
    setLoading(false);
    if (!res.ok) setError(res.error);
    else {
      setSuccess("Account created! You can now sign in.");
      setTimeout(() => navigate({ to: "/login" }), 1200);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-10 bg-muted/40">
      <div className="w-full max-w-md">
        <Link to="/" className="flex items-center gap-2 justify-center mb-8">
          <div className="size-10 rounded-xl gradient-hero flex items-center justify-center">
            <ShieldCheck className="size-5 text-white" />
          </div>
          <div className="text-lg font-semibold">CreditIQ</div>
        </Link>

        <div className="bg-card border border-border rounded-2xl p-7 shadow-(--shadow-card)">
          <h1 className="text-xl font-semibold">Create your account</h1>
          <p className="text-sm text-muted-foreground mt-1">Sign up to access the risk dashboard.</p>

          <form onSubmit={submit} className="mt-6 space-y-4">
            <div>
              <label className="text-xs font-medium text-foreground">Full Name</label>
              <input
                type="text"
                value={fullName}
                onChange={e => setFullName(e.target.value)}
                placeholder="Your name"
                className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
                required
              />
            </div>
            <div>
              <label className="text-xs font-medium text-foreground">Email</label>
              <input
                type="email"
                value={email}
                onChange={e => setEmail(e.target.value)}
                placeholder="you@credit.com"
                className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
                required
              />
            </div>
            <div>
              <label className="text-xs font-medium text-foreground">Phone Number</label>
              <input
                type="tel"
                value={phoneNumber}
                onChange={e => setPhoneNumber(e.target.value)}
                placeholder="+1 555 123 4567"
                className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
                required
              />
            </div>
            <div>
              <label className="text-xs font-medium text-foreground">Password</label>
              <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                placeholder="••••••••"
                className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
                required
              />
            </div>
            <div>
              <label className="text-xs font-medium text-foreground">Confirm Password</label>
              <input
                type="password"
                value={confirmPassword}
                onChange={e => setConfirmPassword(e.target.value)}
                placeholder="••••••••"
                className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
                required
              />
            </div>

            {error && (
              <div className="text-xs rounded-lg px-3 py-2 bg-destructive/10 text-destructive border border-destructive/20">
                {error}
              </div>
            )}
            {success && (
              <div className="text-xs rounded-lg px-3 py-2 bg-success/10 text-success border border-success/20">
                {success}
              </div>
            )}

            <button
              type="submit"
              className="w-full h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition flex items-center justify-center gap-2"
              disabled={loading}
            >
              {loading ? "Signing up..." : <>Sign up <ArrowRight className="size-4" /></>}
            </button>
          </form>
        </div>

        <div className="text-center text-xs text-muted-foreground mt-6 flex flex-col gap-2">
          <Link to="/login" className="hover:text-foreground transition">Already have an account? Sign in</Link>
          <Link to="/" className="hover:text-foreground transition">← Back to home</Link>
        </div>
      </div>
    </div>
  );
}
