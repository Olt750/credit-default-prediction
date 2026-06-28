import { useState, useRef, useEffect } from "react";
import { Search, Bell, ChevronDown, LogOut } from "lucide-react";
import { useNavigate } from "@tanstack/react-router";
import { useAuth } from "@/lib/auth";
import { useNotifications } from "@/lib/notifications";

export function Navbar() {
  const { user, logout } = useAuth();
  const { unreadCount } = useNotifications();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const h = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", h);
    return () => document.removeEventListener("mousedown", h);
  }, []);

  const initials = (user?.name ?? "U")
    .split(" ")
    .map((s) => s[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  const handleLogout = () => {
    logout();
    navigate({ to: "/login" });
  };

  return (
    <header className="h-16 sticky top-0 z-20 border-b border-border bg-card">
      <div className="h-full px-4 lg:px-8 flex items-center gap-4">
        <div className="flex-1 max-w-md relative">
          <Search className="size-4 absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <input
            type="text"
            placeholder="Search clients, predictions, models…"
            className="w-full pl-9 pr-4 h-10 rounded-xl bg-muted/60 border border-transparent text-sm placeholder:text-muted-foreground focus:outline-none focus:bg-background focus:border-border focus:ring-2 focus:ring-ring/30"
          />
        </div>
        <button
          onClick={() => navigate({ to: "/notifications" })}
          className="relative size-10 rounded-xl bg-muted/60 hover:bg-muted transition flex items-center justify-center"
          aria-label="Open notifications"
        >
          <Bell className="size-4" />
          {unreadCount > 0 && (
            <span className="absolute -top-1 -right-1 min-w-5 h-5 px-1 rounded-full bg-destructive text-[10px] leading-5 text-destructive-foreground text-center">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </button>
        <div ref={ref} className="relative">
          <button
            onClick={() => setOpen((o) => !o)}
            className="flex items-center gap-2 pl-1 pr-3 h-10 rounded-xl bg-muted/60 hover:bg-muted transition"
          >
            <div className="size-8 rounded-lg gradient-hero flex items-center justify-center text-xs font-semibold text-white">
              {initials}
            </div>
            <div className="hidden sm:block text-left">
              <div className="text-xs font-medium leading-none">{user?.name ?? "Guest"}</div>
              <div className="text-[10px] text-muted-foreground mt-0.5">{user?.role ?? "—"}</div>
            </div>
            <ChevronDown className="size-3.5 text-muted-foreground" />
          </button>
          {open && (
            <div className="absolute right-0 top-12 w-60 rounded-xl border border-border bg-card shadow-[var(--shadow-card)] p-3 text-sm">
              <div className="px-2 py-1.5">
                <div className="font-medium">{user?.name}</div>
                <div className="text-xs text-muted-foreground">{user?.email}</div>
                <div className="mt-1.5 inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[10px] font-medium bg-primary/10 text-primary">
                  {user?.role}
                </div>
              </div>
              <div className="my-2 h-px bg-border" />
              <button
                onClick={handleLogout}
                className="w-full flex items-center gap-2 px-2 py-2 rounded-lg text-sm hover:bg-muted transition text-destructive"
              >
                <LogOut className="size-4" /> Sign out
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
