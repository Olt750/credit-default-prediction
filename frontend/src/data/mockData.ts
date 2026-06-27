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
  { id: "U-003", name: "Anita Rama", email: "anita.rama@credit.com", role: "Analyst", status: "Active", createdAt: "2026-01-08" },
  { id: "U-004", name: "Bledar Shehu", email: "bledar.shehu@credit.com", role: "User", status: "Disabled", createdAt: "2026-01-22" },
  { id: "U-005", name: "Era Krasniqi", email: "era.krasniqi@credit.com", role: "Analyst", status: "Active", createdAt: "2026-02-11" },
  { id: "U-006", name: "Driton Berisha", email: "driton.berisha@credit.com", role: "User", status: "Invited", createdAt: "2026-03-04" },
  { id: "U-007", name: "Mira Dervishi", email: "mira.dervishi@credit.com", role: "User", status: "Active", createdAt: "2026-03-19" },
  { id: "U-008", name: "Genti Vata", email: "genti.vata@credit.com", role: "Analyst", status: "Active", createdAt: "2026-04-02" },
];
