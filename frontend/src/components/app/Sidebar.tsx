import { Link, useRouterState } from "@tanstack/react-router";
import {
  LayoutDashboard,
  UserSearch,
  Activity,
  GitCompare,
  Network,
  Layers,
  FileBarChart,
  Bell,
  Settings,
  ShieldCheck,
  Users as UsersIcon,
} from "lucide-react";
import { useAuth, type Role } from "@/lib/auth";

type Item = { to: string; label: string; icon: typeof LayoutDashboard; roles: Role[] };

const items: Item[] = [
  { to: "/dashboard", label: "Dashboard", icon: LayoutDashboard, roles: ["Admin", "User"] },
  { to: "/clients", label: "Client Analysis", icon: UserSearch, roles: ["Admin", "User"] },
  { to: "/predictions", label: "Predictions", icon: Activity, roles: ["Admin", "User"] },
  { to: "/models", label: "Models Comparison", icon: GitCompare, roles: ["Admin"] },
  { to: "/neural-network", label: "Neural Network", icon: Network, roles: ["Admin"] },
  { to: "/clustering", label: "Clustering Insights", icon: Layers, roles: ["Admin"] },
  { to: "/reports", label: "Reports", icon: FileBarChart, roles: ["Admin", "User"] },
  { to: "/notifications", label: "Notifications", icon: Bell, roles: ["Admin", "User"] },
  { to: "/users", label: "Users", icon: UsersIcon, roles: ["Admin"] },
  { to: "/settings", label: "Settings", icon: Settings, roles: ["Admin", "User"] },
];

export function Sidebar() {
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const { user } = useAuth();
  const visible = items.filter((i) => !user || i.roles.includes(user.role));
  return (
    <aside className="hidden lg:flex w-64 shrink-0 flex-col bg-sidebar border-r border-sidebar-border h-screen sticky top-0">
      <Link to="/" className="flex items-center gap-2.5 px-6 h-16 border-b border-sidebar-border">
        <div className="size-9 rounded-xl gradient-hero flex items-center justify-center">
          <ShieldCheck className="size-5 text-white" />
        </div>
        <div>
          <div className="text-sm font-semibold text-sidebar-foreground leading-none">CreditIQ</div>
          <div className="text-[10px] uppercase tracking-wider text-muted-foreground mt-1">Risk Platform</div>
        </div>
      </Link>
      <nav className="flex-1 overflow-y-auto px-3 py-4 space-y-0.5">
        {visible.map((it) => {
          const active = pathname === it.to;
          const Icon = it.icon;
          return (
            <Link
              key={it.to}
              to={it.to}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                active
                  ? "bg-primary/10 text-primary font-medium"
                  : "text-sidebar-foreground/80 hover:text-sidebar-foreground hover:bg-sidebar-accent"
              }`}
            >
              <Icon className="size-4" />
              <span>{it.label}</span>
            </Link>
          );
        })}
      </nav>
      <div className="p-4 border-t border-sidebar-border text-xs text-muted-foreground">
        <div className="flex items-center gap-2">
          <span className="size-2 rounded-full bg-success" />
          All systems operational
        </div>
      </div>
    </aside>
  );
}