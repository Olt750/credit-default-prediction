import { Link, createFileRoute } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { Users, AlertTriangle, CheckCircle2, Activity } from "lucide-react";
import {
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Tooltip,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  BarChart,
  Bar,
} from "recharts";
import { PageHeader } from "@/components/app/PageHeader";
import { StatCard } from "@/components/app/StatCard";
import { ChartCard } from "@/components/app/ChartCard";
import { PredictionTable } from "@/components/app/PredictionTable";
import { apiFetch } from "@/lib/api";

export const Route = createFileRoute("/_app/dashboard")({
  component: DashboardPage,
});

const tooltipStyle = {
  background: "var(--popover)",
  border: "1px solid var(--border)",
  borderRadius: 12,
  fontSize: 12,
  color: "var(--foreground)",
};

type DashboardSummary = {
  totalClients: number;
  highRiskClients: number;
  approvedLoans: number;
  averageDefaultRisk: number;
  avgDefaultRisk?: number;
};

type RiskDistribution = {
  name: string;
  value: number;
  color: string;
};

type MonthlyActivity = {
  month: string;
  predictions: number;
  approved: number;
};

type LoanStatus = {
  status: string;
  count: number;
};

const emptyStats: DashboardSummary = {
  totalClients: 0,
  highRiskClients: 0,
  approvedLoans: 0,
  averageDefaultRisk: 0,
};

function DashboardPage() {
  const [stats, setStats] = useState<DashboardSummary>(emptyStats);
  const [riskDistribution, setRiskDistribution] = useState<RiskDistribution[]>([]);
  const [monthlyActivity, setMonthlyActivity] = useState<MonthlyActivity[]>([]);
  const [loanStatus, setLoanStatus] = useState<LoanStatus[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadDashboard() {
      setLoading(true);
      setError(null);

      try {
        const res = await apiFetch("/dashboard");

        if (!res.ok) {
          throw new Error("Failed to load dashboard data.");
        }

        const data = await res.json();
        setStats(data.summary ?? emptyStats);
        setRiskDistribution(data.riskDistribution ?? []);
        setMonthlyActivity(data.monthlyActivity ?? []);
        setLoanStatus(data.loanStatusSummary ?? []);
      } catch (err: any) {
        setError(err.message || "Failed to load dashboard data.");
      } finally {
        setLoading(false);
      }
    }

    loadDashboard();
  }, []);

  return (
    <>
      <PageHeader
        title="Risk Dashboard"
        subtitle="Portfolio-wide credit risk overview, updated with the latest model run."
        actions={
          <Link to="/clients" className="h-10 px-4 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition inline-flex items-center">
            New analysis
          </Link>
        }
      />

      {error && (
        <div className="mb-4 rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Total Clients"
          value={loading ? "..." : stats.totalClients.toLocaleString()}
          icon={Users}
          delta="3.2%"
          tint="primary"
        />
        <StatCard
          label="High Risk Clients"
          value={loading ? "..." : stats.highRiskClients.toLocaleString()}
          icon={AlertTriangle}
          delta="1.4%"
          trend="down"
          tint="destructive"
        />
        <StatCard
          label="Approved Loans"
          value={loading ? "..." : stats.approvedLoans.toLocaleString()}
          icon={CheckCircle2}
          delta="5.1%"
          tint="success"
        />
        <StatCard
          label="Avg. Default Risk"
          value={loading ? "..." : `${stats.averageDefaultRisk ?? stats.avgDefaultRisk ?? 0}%`}
          icon={Activity}
          delta="0.6%"
          trend="down"
          tint="warning"
        />
      </div>

      <div className="grid lg:grid-cols-3 gap-4 mt-4">
        <ChartCard title="Risk distribution" subtitle="Across active portfolio">
          {riskDistribution.some((item) => item.value > 0) ? (
            <>
              <ResponsiveContainer width="100%" height={240}>
                <PieChart>
                  <Pie
                    data={riskDistribution}
                    dataKey="value"
                    innerRadius={55}
                    outerRadius={85}
                    paddingAngle={3}
                  >
                    {riskDistribution.map((entry, index) => (
                      <Cell key={index} fill={entry.color} stroke="none" />
                    ))}
                  </Pie>
                  <Tooltip contentStyle={tooltipStyle} />
                </PieChart>
              </ResponsiveContainer>

              <div className="flex justify-center gap-4 text-xs">
                {riskDistribution.map((item) => (
                  <div key={item.name} className="flex items-center gap-1.5">
                    <span
                      className="size-2 rounded-full"
                      style={{ background: item.color }}
                    />
                    <span className="text-muted-foreground">{item.name}</span>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <div className="p-4 text-center text-muted-foreground">
              No risk distribution data.
            </div>
          )}
        </ChartCard>

        <ChartCard
          title="Monthly prediction activity"
          subtitle="Predictions vs approvals"
          className="lg:col-span-2"
        >
          {monthlyActivity.some((item) => item.predictions > 0 || item.approved > 0) ? (
            <ResponsiveContainer width="100%" height={260}>
              <AreaChart data={monthlyActivity}>
                <defs>
                  <linearGradient id="g1" x1="0" y1="0" x2="0" y2="1">
                    <stop
                      offset="0%"
                      stopColor="var(--primary)"
                      stopOpacity={0.5}
                    />
                    <stop
                      offset="100%"
                      stopColor="var(--primary)"
                      stopOpacity={0}
                    />
                  </linearGradient>
                  <linearGradient id="g2" x1="0" y1="0" x2="0" y2="1">
                    <stop
                      offset="0%"
                      stopColor="var(--accent)"
                      stopOpacity={0.4}
                    />
                    <stop
                      offset="100%"
                      stopColor="var(--accent)"
                      stopOpacity={0}
                    />
                  </linearGradient>
                </defs>

                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis
                  dataKey="month"
                  stroke="var(--muted-foreground)"
                  fontSize={11}
                />
                <YAxis stroke="var(--muted-foreground)" fontSize={11} />
                <Tooltip contentStyle={tooltipStyle} />
                <Area
                  type="monotone"
                  dataKey="predictions"
                  stroke="var(--primary)"
                  fill="url(#g1)"
                  strokeWidth={2}
                />
                <Area
                  type="monotone"
                  dataKey="approved"
                  stroke="var(--accent)"
                  fill="url(#g2)"
                  strokeWidth={2}
                />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <div className="p-4 text-center text-muted-foreground">
              No monthly activity data.
            </div>
          )}
        </ChartCard>
      </div>

      <div className="grid lg:grid-cols-3 gap-4 mt-4">
        <ChartCard title="Loan status summary">
          {loanStatus.some((item) => item.count > 0) ? (
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={loanStatus}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                <XAxis
                  dataKey="status"
                  stroke="var(--muted-foreground)"
                  fontSize={11}
                />
                <YAxis stroke="var(--muted-foreground)" fontSize={11} />
                <Tooltip contentStyle={tooltipStyle} />
                <Bar
                  dataKey="count"
                  radius={[8, 8, 0, 0]}
                  fill="var(--primary)"
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="p-4 text-center text-muted-foreground">
              No loan status data.
            </div>
          )}
        </ChartCard>

        <ChartCard
          title="Recent predictions"
          subtitle="Most recent model output"
          className="lg:col-span-2"
        >
          <PredictionTable limit={5} />
        </ChartCard>
      </div>
    </>
  );
}
