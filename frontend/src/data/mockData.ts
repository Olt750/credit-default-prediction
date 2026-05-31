export const stats = {
  totalClients: 12480,
  highRiskClients: 1842,
  approvedLoans: 8961,
  avgDefaultRisk: 23.4,
};

export const riskDistribution = [
  { name: "Low", value: 6420, color: "var(--color-success)" },
  { name: "Medium", value: 4218, color: "var(--color-warning)" },
  { name: "High", value: 1842, color: "var(--color-destructive)" },
];

export const monthlyActivity = [
  { month: "Jan", predictions: 420, approved: 290 },
  { month: "Feb", predictions: 510, approved: 360 },
  { month: "Mar", predictions: 680, approved: 470 },
  { month: "Apr", predictions: 740, approved: 510 },
  { month: "May", predictions: 820, approved: 590 },
  { month: "Jun", predictions: 910, approved: 640 },
  { month: "Jul", predictions: 980, approved: 700 },
  { month: "Aug", predictions: 1120, approved: 810 },
];

export const loanStatus = [
  { status: "Approved", count: 8961 },
  { status: "Pending", count: 1240 },
  { status: "Rejected", count: 2279 },
];

export type RiskLevel = "Low" | "Medium" | "High";

export const recentPredictions: {
  id: string;
  client: string;
  amount: number;
  score: number;
  level: RiskLevel;
  model: string;
  date: string;
  status: "Approved" | "Pending" | "Rejected";
}[] = [
  { id: "PR-1042", client: "Arlind Hoxha", amount: 18500, score: 82, level: "High", model: "Random Forest", date: "2026-05-22", status: "Rejected" },
  { id: "PR-1041", client: "Era Krasniqi", amount: 9200, score: 24, level: "Low", model: "Neural Network", date: "2026-05-22", status: "Approved" },
  { id: "PR-1040", client: "Bledar Shehu", amount: 45000, score: 61, level: "Medium", model: "Logistic Regression", date: "2026-05-21", status: "Pending" },
  { id: "PR-1039", client: "Mira Dervishi", amount: 12000, score: 18, level: "Low", model: "Random Forest", date: "2026-05-21", status: "Approved" },
  { id: "PR-1038", client: "Genti Vata", amount: 30500, score: 74, level: "High", model: "Decision Tree", date: "2026-05-20", status: "Rejected" },
  { id: "PR-1037", client: "Albana Leka", amount: 7800, score: 41, level: "Medium", model: "Neural Network", date: "2026-05-20", status: "Approved" },
  { id: "PR-1036", client: "Driton Berisha", amount: 22000, score: 33, level: "Low", model: "Random Forest", date: "2026-05-19", status: "Approved" },
  { id: "PR-1035", client: "Saimir Gjoka", amount: 55000, score: 88, level: "High", model: "Neural Network", date: "2026-05-19", status: "Rejected" },
];

export const modelMetrics = [
  { model: "Logistic Regression", accuracy: 0.84, precision: 0.81, recall: 0.78, f1: 0.79 },
  { model: "Decision Tree", accuracy: 0.86, precision: 0.83, recall: 0.82, f1: 0.82 },
  { model: "Random Forest", accuracy: 0.92, precision: 0.91, recall: 0.89, f1: 0.90 },
  { model: "Neural Network", accuracy: 0.93, precision: 0.92, recall: 0.90, f1: 0.91 },
];

export const confusionMatrices = [
  { model: "Logistic Regression", tp: 412, tn: 980, fp: 88, fn: 120 },
  { model: "Decision Tree", tp: 438, tn: 992, fp: 76, fn: 94 },
  { model: "Random Forest", tp: 478, tn: 1015, fp: 53, fn: 54 },
  { model: "Neural Network", tp: 482, tn: 1020, fp: 48, fn: 50 },
];

export const clusters = [
  { name: "Low Risk Group", size: 6420, avgScore: 21, color: "var(--color-success)" },
  { name: "Medium Risk Group", size: 4218, avgScore: 52, color: "var(--color-warning)" },
  { name: "High Risk Group", size: 1842, avgScore: 81, color: "var(--color-destructive)" },
];

export const clusterPoints = Array.from({ length: 120 }, (_, i) => {
  const c = i % 3;
  const cx = [25, 55, 80][c];
  const cy = [30, 55, 80][c];
  return {
    x: cx + (Math.random() - 0.5) * 18,
    y: cy + (Math.random() - 0.5) * 18,
    cluster: ["Low", "Medium", "High"][c],
  };
});

export const notifications = [
  { id: 1, type: "danger", title: "New high-risk prediction detected", desc: "Client Saimir Gjoka — score 88%", time: "2 min ago" },
  { id: 2, type: "info", title: "Model performance updated", desc: "Random Forest retrained — accuracy 92.4%", time: "1 hr ago" },
  { id: 3, type: "success", title: "Report generated successfully", desc: "Monthly risk report ready for download", time: "3 hr ago" },
  { id: 4, type: "warning", title: "Threshold drift detected", desc: "Medium-risk band shifting upward in recent batch", time: "Yesterday" },
  { id: 5, type: "info", title: "Weekly digest", desc: "812 predictions processed in last 7 days", time: "2 days ago" },
];

export const nnArchitectures = [
  {
    name: "Architecture A",
    layers: ["Input (10)", "Dense 32 — ReLU", "Dense 16 — ReLU", "Output — Sigmoid"],
    accuracy: 0.89,
    loss: 0.31,
  },
  {
    name: "Architecture B",
    layers: ["Input (10)", "Dense 64 — ReLU", "Dense 32 — ReLU", "Dense 16 — ReLU", "Output — Sigmoid"],
    accuracy: 0.93,
    loss: 0.22,
  },
];

export type AppUserRow = {
  id: string;
  name: string;
  email: string;
  role: "Admin" | "User" | "Analyst";
  status: "Active" | "Disabled" | "Invited";
  createdAt: string;
};

export const appUsers: AppUserRow[] = [
  { id: "U-001", name: "Admin Account", email: "admin@credit.com", role: "Admin", status: "Active", createdAt: "2025-11-02" },
  { id: "U-002", name: "Standard User", email: "user@credit.com", role: "User", status: "Active", createdAt: "2025-12-14" },
  { id: "U-003", name: "Anita Rama", email: "anita.rama@credit.com", role: "Analyst", status: "Active", createdAt: "2026-01-08" },
  { id: "U-004", name: "Bledar Shehu", email: "bledar.shehu@credit.com", role: "User", status: "Disabled", createdAt: "2026-01-22" },
  { id: "U-005", name: "Era Krasniqi", email: "era.krasniqi@credit.com", role: "Analyst", status: "Active", createdAt: "2026-02-11" },
  { id: "U-006", name: "Driton Berisha", email: "driton.berisha@credit.com", role: "User", status: "Invited", createdAt: "2026-03-04" },
  { id: "U-007", name: "Mira Dervishi", email: "mira.dervishi@credit.com", role: "User", status: "Active", createdAt: "2026-03-19" },
  { id: "U-008", name: "Genti Vata", email: "genti.vata@credit.com", role: "Analyst", status: "Active", createdAt: "2026-04-02" },
];