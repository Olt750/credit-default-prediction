import { createFileRoute } from "@tanstack/react-router";
import { Users, AlertTriangle, CheckCircle2, Activity } from "lucide-react";
import {
  ResponsiveContainer, PieChart, Pie, Cell, Tooltip,
  AreaChart, Area, XAxis, YAxis, CartesianGrid, BarChart, Bar,
} from "recharts";
import { PageHeader } from "@/components/app/PageHeader";
import { StatCard } from "@/components/app/StatCard";
import { ChartCard } from "@/components/app/ChartCard";
import { PredictionTable } from "@/components/app/PredictionTable";
import { stats, riskDistribution, monthlyActivity, loanStatus } from "@/data/mockData";

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

function DashboardPage() {
  return (
    <>
      <PageHeader
        title="Risk Dashboard"
        subtitle="Portfolio-wide credit risk overview, updated with the latest model run."
        actions={
          <button className="h-10 px-4 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition">
            New analysis
          </button>
        }
      />

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="Total Clients" value={stats.totalClients.toLocaleString()} icon={Users} delta="3.2%" tint="primary" />
        <StatCard label="High Risk Clients" value={stats.highRiskClients.toLocaleString()} icon={AlertTriangle} delta="1.4%" trend="down" tint="destructive" />
        <StatCard label="Approved Loans" value={stats.approvedLoans.toLocaleString()} icon={CheckCircle2} delta="5.1%" tint="success" />
        <StatCard label="Avg. Default Risk" value={`${stats.avgDefaultRisk}%`} icon={Activity} delta="0.6%" trend="down" tint="warning" />
      </div>

      <div className="grid lg:grid-cols-3 gap-4 mt-4">
        <ChartCard title="Risk distribution" subtitle="Across active portfolio">
          <ResponsiveContainer width="100%" height={240}>
            <PieChart>
              <Pie data={riskDistribution} dataKey="value" innerRadius={55} outerRadius={85} paddingAngle={3}>
                {riskDistribution.map((e, i) => <Cell key={i} fill={e.color} stroke="none" />)}
              </Pie>
              <Tooltip contentStyle={tooltipStyle} />
            </PieChart>
          </ResponsiveContainer>
          <div className="flex justify-center gap-4 text-xs">
            {riskDistribution.map((r) => (
              <div key={r.name} className="flex items-center gap-1.5">
                <span className="size-2 rounded-full" style={{ background: r.color }} />
                <span className="text-muted-foreground">{r.name}</span>
              </div>
            ))}
          </div>
        </ChartCard>

        <ChartCard title="Monthly prediction activity" subtitle="Predictions vs approvals" className="lg:col-span-2">
          <ResponsiveContainer width="100%" height={260}>
            <AreaChart data={monthlyActivity}>
              <defs>
                <linearGradient id="g1" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="var(--primary)" stopOpacity={0.5} />
                  <stop offset="100%" stopColor="var(--primary)" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="g2" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="var(--accent)" stopOpacity={0.4} />
                  <stop offset="100%" stopColor="var(--accent)" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="month" stroke="var(--muted-foreground)" fontSize={11} />
              <YAxis stroke="var(--muted-foreground)" fontSize={11} />
              <Tooltip contentStyle={tooltipStyle} />
              <Area type="monotone" dataKey="predictions" stroke="var(--primary)" fill="url(#g1)" strokeWidth={2} />
              <Area type="monotone" dataKey="approved" stroke="var(--accent)" fill="url(#g2)" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </ChartCard>
      </div>

      <div className="grid lg:grid-cols-3 gap-4 mt-4">
        <ChartCard title="Loan status summary">
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={loanStatus}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis dataKey="status" stroke="var(--muted-foreground)" fontSize={11} />
              <YAxis stroke="var(--muted-foreground)" fontSize={11} />
              <Tooltip contentStyle={tooltipStyle} />
              <Bar dataKey="count" radius={[8, 8, 0, 0]} fill="var(--primary)" />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Recent predictions" subtitle="Most recent model output" className="lg:col-span-2">
          <PredictionTable limit={5} />
        </ChartCard>
      </div>
    </>
  );
}