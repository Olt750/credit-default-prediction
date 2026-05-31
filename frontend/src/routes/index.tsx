import { createFileRoute, Link } from "@tanstack/react-router";
import { useAuth } from "@/lib/auth";
import {
  ShieldCheck,
  ArrowRight,
  LineChart,
  Database,
  Brain,
  CheckCircle2,
  Zap,
  TrendingDown,
  Layers,
  Sparkles,
} from "lucide-react";

export const Route = createFileRoute("/")({
  component: Index,
});

function Index() {
  const { user } = useAuth();
  return (
    <div className="min-h-screen">
      {/* Nav */}
      <header className="sticky top-0 z-30 bg-card/80 backdrop-blur-xl border-b border-border">
        <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2">
            <div className="size-9 rounded-xl gradient-hero flex items-center justify-center shadow-[var(--shadow-glow)]">
              <ShieldCheck className="size-5 text-white" />
            </div>
            <div>
              <div className="text-sm font-semibold leading-none">CreditIQ</div>
              <div className="text-[10px] uppercase tracking-wider text-muted-foreground mt-1">Risk Intelligence</div>
            </div>
          </Link>
          <nav className="hidden md:flex items-center gap-8 text-sm text-muted-foreground">
            <a href="#why" className="hover:text-foreground transition">Why us</a>
            <a href="#how" className="hover:text-foreground transition">How it works</a>
            <a href="#benefits" className="hover:text-foreground transition">Benefits</a>
          </nav>
          <div className="flex items-center gap-2">
            {user ? (
              <Link to="/dashboard" className="inline-flex h-9 px-4 items-center gap-1.5 rounded-lg text-sm font-medium bg-primary text-primary-foreground hover:opacity-90 transition">
                Open Dashboard <ArrowRight className="size-3.5" />
              </Link>
            ) : (
              <>
                <Link to="/login" className="hidden sm:inline-flex h-9 px-4 items-center rounded-lg text-sm text-foreground hover:bg-muted transition">
                  Sign in
                </Link>
                <Link to="/login" className="inline-flex h-9 px-4 items-center gap-1.5 rounded-lg text-sm font-medium bg-primary text-primary-foreground hover:opacity-90 transition">
                  Get started <ArrowRight className="size-3.5" />
                </Link>
              </>
            )}
          </div>
        </div>
      </header>

      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="absolute inset-0 -z-10"
          style={{
            backgroundImage:
              "radial-gradient(circle at 20% 30%, oklch(0.85 0.08 245 / 0.5), transparent 55%), radial-gradient(circle at 80% 70%, oklch(0.88 0.07 220 / 0.45), transparent 55%)",
          }}
        />
        <div className="max-w-7xl mx-auto px-6 pt-20 pb-24 lg:pt-32 lg:pb-32">
          <div className="max-w-3xl">
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-card border border-border text-xs text-muted-foreground mb-6 shadow-[var(--shadow-card)]">
              <Sparkles className="size-3.5 text-accent" />
              ML-powered credit risk for modern banks
            </div>
            <h1 className="text-4xl md:text-6xl lg:text-7xl font-semibold tracking-tight leading-[1.05]">
              Credit Default Prediction <br />
              using <span className="gradient-text">Machine Learning</span>
            </h1>
            <p className="mt-6 text-lg text-muted-foreground max-w-2xl">
              A decision-support platform that helps financial institutions identify high-risk
              loan applicants in seconds — combining classical models and neural networks for
              transparent, defensible risk scores.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Link to={user ? "/clients" : "/login"} className="inline-flex h-12 px-6 items-center gap-2 rounded-xl font-medium bg-primary text-primary-foreground hover:opacity-90 transition shadow-[var(--shadow-glow)]">
                {user ? "Start Analysis" : "Sign in to start"} <ArrowRight className="size-4" />
              </Link>
              <Link to={user ? "/dashboard" : "/login"} className="inline-flex h-12 px-6 items-center gap-2 rounded-xl font-medium bg-card border border-border hover:bg-muted transition">
                View Dashboard
              </Link>
            </div>

            {/* Trust stats */}
            <div className="mt-14 grid grid-cols-2 md:grid-cols-4 gap-6">
              {[
                { label: "Avg. accuracy", value: "93%" },
                { label: "Decision time", value: "< 2s" },
                { label: "Models", value: "5+" },
                { label: "Banks piloting", value: "12" },
              ].map((s) => (
                <div key={s.label}>
                  <div className="text-3xl font-semibold gradient-text">{s.value}</div>
                  <div className="text-xs uppercase tracking-wider text-muted-foreground mt-1">{s.label}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Why */}
      <section id="why" className="max-w-7xl mx-auto px-6 py-20">
        <div className="max-w-2xl">
          <div className="text-xs uppercase tracking-wider text-primary">Why it matters</div>
          <h2 className="text-3xl md:text-4xl font-semibold tracking-tight mt-3">
            Why credit risk prediction matters
          </h2>
          <p className="text-muted-foreground mt-4">
            Manual underwriting is slow, inconsistent, and exposed to bias. A data-driven
            approach makes risk visible, auditable, and continuously improving.
          </p>
        </div>
        <div className="mt-10 grid md:grid-cols-3 gap-5">
          {[
            { icon: TrendingDown, title: "Reduce default losses", desc: "Catch high-risk applicants before disbursement using historical patterns." },
            { icon: Zap, title: "Decide in seconds", desc: "Score new applications in real time instead of waiting on committee review." },
            { icon: CheckCircle2, title: "Auditable decisions", desc: "Every prediction comes with feature-level explanations regulators can verify." },
          ].map((c) => (
            <div key={c.title} className="glass rounded-2xl p-6">
              <div className="size-11 rounded-xl bg-gradient-to-br from-primary/30 to-primary/5 text-primary flex items-center justify-center">
                <c.icon className="size-5" />
              </div>
              <div className="mt-5 font-semibold">{c.title}</div>
              <div className="text-sm text-muted-foreground mt-1.5">{c.desc}</div>
            </div>
          ))}
        </div>
      </section>

      {/* How */}
      <section id="how" className="max-w-7xl mx-auto px-6 py-20">
        <div className="text-center max-w-2xl mx-auto">
          <div className="text-xs uppercase tracking-wider text-accent">How the system works</div>
          <h2 className="text-3xl md:text-4xl font-semibold tracking-tight mt-3">
            From application to decision in four steps
          </h2>
        </div>
        <div className="mt-12 grid md:grid-cols-4 gap-5">
          {[
            { icon: Database, title: "Data Input", desc: "Collect applicant financial profile." },
            { icon: Brain, title: "ML Analysis", desc: "Run through ensemble of trained classifiers." },
            { icon: LineChart, title: "Risk Score", desc: "Produce calibrated probability of default." },
            { icon: ShieldCheck, title: "Decision Support", desc: "Recommend approve, review, or reject." },
          ].map((s, i) => (
            <div key={s.title} className="glass rounded-2xl p-6 relative">
              <div className="absolute top-4 right-4 text-xs text-muted-foreground font-mono">0{i + 1}</div>
              <div className="size-11 rounded-xl gradient-hero flex items-center justify-center text-white">
                <s.icon className="size-5" />
              </div>
              <div className="mt-5 font-semibold">{s.title}</div>
              <div className="text-sm text-muted-foreground mt-1.5">{s.desc}</div>
            </div>
          ))}
        </div>
      </section>

      {/* Benefits */}
      <section id="benefits" className="max-w-7xl mx-auto px-6 py-20">
        <div className="grid lg:grid-cols-2 gap-12 items-center">
          <div>
            <div className="text-xs uppercase tracking-wider text-primary">Key benefits</div>
            <h2 className="text-3xl md:text-4xl font-semibold tracking-tight mt-3">
              Built for risk teams who can't afford to guess
            </h2>
            <p className="text-muted-foreground mt-4">
              CreditIQ blends interpretable models with deep neural networks, giving your team
              both speed and the ability to explain every decision.
            </p>
            <ul className="mt-8 space-y-3">
              {[
                "Faster credit decisions across branches",
                "Reduced financial loss from defaults",
                "Automated, consistent risk analysis",
                "Higher prediction accuracy vs. rules-only",
                "Cluster-level portfolio insights",
              ].map((b) => (
                <li key={b} className="flex items-start gap-3">
                  <CheckCircle2 className="size-5 text-success shrink-0 mt-0.5" />
                  <span>{b}</span>
                </li>
              ))}
            </ul>
          </div>
          <div className="glass rounded-3xl p-6 lg:p-8">
            <div className="flex items-center justify-between">
              <div>
                <div className="text-xs text-muted-foreground">Sample prediction</div>
                <div className="text-2xl font-semibold mt-1">Client #4821</div>
              </div>
              <div className="text-right">
                <div className="text-xs text-muted-foreground">Risk Score</div>
                <div className="text-3xl font-semibold text-destructive">78%</div>
              </div>
            </div>
            <div className="mt-6 h-2 rounded-full bg-muted overflow-hidden">
              <div className="h-full w-[78%] bg-gradient-to-r from-warning to-destructive" />
            </div>
            <div className="mt-8 grid grid-cols-2 gap-4 text-sm">
              <div className="rounded-xl bg-muted/40 p-4">
                <div className="text-xs text-muted-foreground">Recommendation</div>
                <div className="font-medium mt-1">Manual Review</div>
              </div>
              <div className="rounded-xl bg-muted/40 p-4">
                <div className="text-xs text-muted-foreground">Top factor</div>
                <div className="font-medium mt-1">Credit history</div>
              </div>
              <div className="rounded-xl bg-muted/40 p-4">
                <div className="text-xs text-muted-foreground">Model</div>
                <div className="font-medium mt-1">Random Forest</div>
              </div>
              <div className="rounded-xl bg-muted/40 p-4">
                <div className="text-xs text-muted-foreground">Confidence</div>
                <div className="font-medium mt-1">0.92</div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="max-w-7xl mx-auto px-6 pb-24">
        <div className="rounded-3xl p-10 lg:p-14 text-center relative overflow-hidden gradient-hero">
          <Layers className="size-10 mx-auto text-white/90" />
          <h3 className="text-2xl md:text-3xl font-semibold mt-4 text-white">
            Explore the platform with mock data
          </h3>
          <p className="text-white/85 mt-3 max-w-xl mx-auto">
            Walk through dashboards, predictions, and model comparisons — everything you need
            before connecting your own data pipeline.
          </p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <Link to={user ? "/dashboard" : "/login"} className="inline-flex h-12 px-6 items-center rounded-xl font-medium bg-white text-primary hover:bg-white/90 transition">
              Open Dashboard
            </Link>
            <Link to={user ? "/clients" : "/login"} className="inline-flex h-12 px-6 items-center rounded-xl font-medium border border-white/50 text-white hover:bg-white/10 transition">
              Try a prediction
            </Link>
          </div>
        </div>
      </section>

      <footer className="border-t border-border py-8">
        <div className="max-w-7xl mx-auto px-6 flex flex-wrap justify-between gap-4 text-xs text-muted-foreground">
          <div>© 2026 CreditIQ — Frontend template, mock data only.</div>
          <div>Parashikimi i Mospagimit të Kredisë</div>
        </div>
      </footer>
    </div>
  );
}
