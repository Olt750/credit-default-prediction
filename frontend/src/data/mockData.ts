export const notifications = [
  { id: 1, type: "danger", title: "New high-risk prediction detected", desc: "Client Saimir Gjoka - score 88%", time: "2 min ago" },
  { id: 2, type: "info", title: "Model performance updated", desc: "Latest training results are available", time: "1 hr ago" },
  { id: 3, type: "success", title: "Report generated successfully", desc: "Monthly risk report ready for download", time: "3 hr ago" },
  { id: 4, type: "warning", title: "Threshold drift detected", desc: "Medium-risk band shifting upward in recent batch", time: "Yesterday" },
  { id: 5, type: "info", title: "Weekly digest", desc: "812 predictions processed in last 7 days", time: "2 days ago" },
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
