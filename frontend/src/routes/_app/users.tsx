import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { Eye, Pencil, Ban, UserPlus } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/lib/auth";

export const Route = createFileRoute("/_app/users")({
  component: UsersPage,
});

const statusStyle: Record<string, string> = {
  Active: "bg-success/10 text-success border-success/20",
  Disabled: "bg-muted text-muted-foreground border-border",
  Invited: "bg-warning/15 text-warning border-warning/20",
};

const roleStyle: Record<string, string> = {
  Admin: "bg-primary/10 text-primary",
  Analyst: "bg-accent/15 text-accent",
  User: "bg-muted text-muted-foreground",
};

type AppUserRow = {
  id: string;
  fullName?: string;
  name?: string;
  email: string;
  role: "Admin" | "User" | "Analyst";
  createdAt?: string;
};

function UsersPage() {
  const { user, ready } = useAuth();
  const navigate = useNavigate();
  const [users, setUsers] = useState<AppUserRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (ready && user && user.role !== "Admin") navigate({ to: "/dashboard" });
  }, [ready, user, navigate]);

  useEffect(() => {
    if (!ready || !user || user.role !== "Admin") return;

    async function loadUsers() {
      setLoading(true);
      setError(null);

      try {
        const res = await apiFetch("/admin/users");

        if (!res.ok) {
          throw new Error("Failed to load users.");
        }

        setUsers(await res.json());
      } catch (err: any) {
        setError(err.message || "Failed to load users.");
      } finally {
        setLoading(false);
      }
    }

    loadUsers();
  }, [ready, user]);

  if (!user || user.role !== "Admin") return null;

  const handle = (action: string, name: string) =>
    window.alert(`${action} — ${name}\n(mock action, no backend)`);

  return (
    <>
      <PageHeader
        title="Users"
        subtitle="Manage platform members and their roles. Admin only."
        actions={
          <button className="h-10 px-4 rounded-xl bg-primary text-primary-foreground text-sm font-medium flex items-center gap-2 hover:opacity-90 transition">
            <UserPlus className="size-4" /> Invite user
          </button>
        }
      />

      {error && (
        <div className="mb-4 rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <div className="bg-card border border-border rounded-2xl shadow-[var(--shadow-card)] overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-xs uppercase tracking-wider text-muted-foreground border-b border-border bg-muted/40">
                <th className="py-3 px-5 font-medium">Name</th>
                <th className="py-3 pr-4 font-medium">Email</th>
                <th className="py-3 pr-4 font-medium">Role</th>
                <th className="py-3 pr-4 font-medium">Status</th>
                <th className="py-3 pr-4 font-medium">Created At</th>
                <th className="py-3 pr-5 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading && (
                <tr>
                  <td colSpan={6} className="py-6 px-5 text-center text-muted-foreground">
                    Loading users...
                  </td>
                </tr>
              )}
              {!loading && !users.length && (
                <tr>
                  <td colSpan={6} className="py-6 px-5 text-center text-muted-foreground">
                    No users found.
                  </td>
                </tr>
              )}
              {!loading && users.map((u) => {
                const name = u.fullName ?? u.name ?? u.email;

                return (
                  <tr key={u.id} className="border-b border-border last:border-0 hover:bg-muted/30 transition">
                    <td className="py-3 px-5">
                      <div className="flex items-center gap-3">
                        <div className="size-8 rounded-lg bg-primary/10 text-primary text-xs font-semibold flex items-center justify-center">
                          {name.split(" ").map((s) => s[0]).join("").slice(0, 2)}
                        </div>
                        <div className="font-medium">{name}</div>
                      </div>
                    </td>
                    <td className="py-3 pr-4 text-muted-foreground">{u.email}</td>
                    <td className="py-3 pr-4">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${roleStyle[u.role] ?? ""}`}>
                        {u.role}
                      </span>
                    </td>
                    <td className="py-3 pr-4">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium border ${statusStyle.Active}`}>
                        Active
                      </span>
                    </td>
                    <td className="py-3 pr-4 text-muted-foreground">
                      {u.createdAt ? new Date(u.createdAt).toLocaleDateString() : ""}
                    </td>
                    <td className="py-3 pr-5">
                      <div className="flex items-center gap-1 justify-end">
                        <button
                          onClick={() => handle("View", name)}
                          className="size-8 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground flex items-center justify-center transition"
                          title="View"
                        >
                          <Eye className="size-4" />
                        </button>
                        <button
                          onClick={() => handle("Edit", name)}
                          className="size-8 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground flex items-center justify-center transition"
                          title="Edit"
                        >
                          <Pencil className="size-4" />
                        </button>
                        <button
                          onClick={() => handle("Disable", name)}
                          className="size-8 rounded-lg hover:bg-destructive/10 text-muted-foreground hover:text-destructive flex items-center justify-center transition"
                          title="Disable"
                        >
                          <Ban className="size-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
