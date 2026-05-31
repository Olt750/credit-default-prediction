import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { useEffect, useState, type FormEvent } from "react";
import { ShieldCheck, ArrowRight } from "lucide-react";
import { useAuth } from "@/lib/auth";

export const Route = createFileRoute("/login")({
  component: LoginPage,
});

function LoginPage() {
  const { user, ready, login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (ready && user) navigate({ to: "/dashboard" });
  }, [ready, user, navigate]);

  const submit = (e: FormEvent) => {
    e.preventDefault();
    const res = login(email, password);
    if (!res.ok) setError(res.error);
    else navigate({ to: "/dashboard" });
  };

  const fill = (e: string, p: string) => {
    setEmail(e);
    setPassword(p);
    setError(null);
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

        <div className="bg-card border border-border rounded-2xl p-7 shadow-[var(--shadow-card)]">
          <h1 className="text-xl font-semibold">Welcome back</h1>
          <p className="text-sm text-muted-foreground mt-1">Sign in to access the risk dashboard.</p>

          <form onSubmit={submit} className="mt-6 space-y-4">
            <div>
              <label className="text-xs font-medium text-foreground">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@credit.com"
                className="mt-1.5 w-full h-11 px-3 rounded-xl bg-background border border-input text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
                required
              />
            </div>
            <div>
              <label className="text-xs font-medium text-foreground">Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
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

            <button
              type="submit"
              className="w-full h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition flex items-center justify-center gap-2"
            >
              Sign in <ArrowRight className="size-4" />
            </button>
          </form>

          <div className="mt-6 pt-5 border-t border-border">
            <div className="text-xs uppercase tracking-wider text-muted-foreground mb-2">Demo accounts</div>
            <div className="grid grid-cols-2 gap-2 text-xs">
              <button
                type="button"
                onClick={() => fill("user@credit.com", "user123")}
                className="rounded-lg border border-border p-3 text-left hover:bg-muted transition"
              >
                <div className="font-medium">User</div>
                <div className="text-muted-foreground">user@credit.com</div>
              </button>
              <button
                type="button"
                onClick={() => fill("admin@credit.com", "admin123")}
                className="rounded-lg border border-border p-3 text-left hover:bg-muted transition"
              >
                <div className="font-medium">Admin</div>
                <div className="text-muted-foreground">admin@credit.com</div>
              </button>
            </div>
          </div>
        </div>

        <div className="text-center text-xs text-muted-foreground mt-6">
          <Link to="/" className="hover:text-foreground transition">← Back to home</Link>
        </div>
      </div>
    </div>
  );
}