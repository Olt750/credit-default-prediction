import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useMemo, useState } from "react";
import { Ban, CheckCircle2, Eye, Pencil, Save, UserPlus, X } from "lucide-react";
import { PageHeader } from "@/components/app/PageHeader";
import { useAuth } from "@/lib/auth";
import { apiFetch } from "@/lib/api";
import { searchList } from "@/services/additionalFeaturesApi";

export const Route = createFileRoute("/_app/users")({
  component: UsersPage,
});

const statusStyle: Record<string, string> = {
  Active: "bg-success/10 text-success border-success/20",
  Disabled: "bg-muted text-muted-foreground border-border",
};

const roleStyle: Record<string, string> = {
  Admin: "bg-primary/10 text-primary",
  Manager: "bg-accent/15 text-accent",
  User: "bg-muted text-muted-foreground",
};

type AppUserRow = {
  id: string;
  fullName?: string;
  name?: string;
  email: string;
  phoneNumber?: string | null;
  role: "Admin" | "Manager" | "User" | string;
  roles?: string[];
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
  predictionCount?: number;
  notificationCount?: number;
  hasClientProfile?: boolean;
};

type ToastState = { type: "success" | "error"; message: string } | null;
type ModalMode = "view" | "edit" | "invite" | null;

const emptyForm = {
  fullName: "",
  email: "",
  phoneNumber: "",
  password: "",
  roles: ["User"],
};

function UsersPage() {
  const { user, ready } = useAuth();
  const navigate = useNavigate();
  const [users, setUsers] = useState<AppUserRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<ToastState>(null);
  const [keyword, setKeyword] = useState("");
  const [role, setRole] = useState("");
  const [status, setStatus] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [sortBy, setSortBy] = useState("createdAt");
  const [sortDirection, setSortDirection] = useState("desc");
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [selectedUser, setSelectedUser] = useState<AppUserRow | null>(null);
  const [form, setForm] = useState(emptyForm);

  useEffect(() => {
    if (ready && user && user.role !== "Admin") navigate({ to: "/dashboard" });
  }, [ready, user, navigate]);

  useEffect(() => {
    if (!ready || !user || user.role !== "Admin") return;
    void loadUsers();
  }, [ready, user, keyword, role, status, page, sortBy, sortDirection]);

  const currentName = selectedUser?.fullName ?? selectedUser?.name ?? selectedUser?.email ?? "";
  const availableRoles = useMemo(() => ["Admin", "Manager", "User"], []);

  if (!user || user.role !== "Admin") return null;

  async function loadUsers() {
    setLoading(true);
    setError(null);

    try {
      const data = await searchList<AppUserRow>("users", {
        keyword,
        role,
        isActive: status === "" ? undefined : status === "active",
        page,
        pageSize: 10,
        sortBy,
        sortDirection,
      });
      setUsers(data.items);
      setTotalPages(data.totalPages || 1);
    } catch (err: any) {
      setError(readError(err, "Failed to load users."));
    } finally {
      setLoading(false);
    }
  }

  async function openDetails(mode: Exclude<ModalMode, "invite" | null>, row: AppUserRow) {
    setSaving(true);
    setToast(null);
    try {
      const detail = await requestJson<AppUserRow>(`/admin/users/${row.id}`);
      setSelectedUser(detail);
      setForm({
        fullName: detail.fullName ?? detail.name ?? "",
        email: detail.email,
        phoneNumber: detail.phoneNumber ?? "",
        password: "",
        roles: detail.roles?.length ? detail.roles : [detail.role || "User"],
      });
      setModalMode(mode);
    } catch (err: any) {
      setToast({ type: "error", message: readError(err, "Failed to load user details.") });
    } finally {
      setSaving(false);
    }
  }

  function openInvite() {
    setSelectedUser(null);
    setForm(emptyForm);
    setModalMode("invite");
  }

  function closeModal() {
    setModalMode(null);
    setSelectedUser(null);
    setForm(emptyForm);
  }

  async function saveInvite() {
    setSaving(true);
    setToast(null);
    try {
      await requestJson<AppUserRow>("/admin/users/invite", {
        method: "POST",
        body: JSON.stringify({
          fullName: form.fullName,
          email: form.email,
          phoneNumber: form.phoneNumber || null,
          password: form.password || undefined,
          roles: form.roles,
        }),
      });
      setToast({ type: "success", message: "User invited successfully." });
      closeModal();
      await loadUsers();
    } catch (err: any) {
      setToast({ type: "error", message: readError(err, "Failed to invite user.") });
    } finally {
      setSaving(false);
    }
  }

  async function saveEdit() {
    if (!selectedUser) return;

    setSaving(true);
    setToast(null);
    try {
      await requestJson<AppUserRow>(`/admin/users/${selectedUser.id}`, {
        method: "PUT",
        body: JSON.stringify({
          fullName: form.fullName,
          email: form.email,
          phoneNumber: form.phoneNumber || null,
        }),
      });
      await requestJson<AppUserRow>(`/admin/users/${selectedUser.id}/roles`, {
        method: "PUT",
        body: JSON.stringify({ roles: form.roles }),
      });
      setToast({ type: "success", message: "User updated successfully." });
      closeModal();
      await loadUsers();
    } catch (err: any) {
      setToast({ type: "error", message: readError(err, "Failed to update user.") });
    } finally {
      setSaving(false);
    }
  }

  async function setUserStatus(row: AppUserRow, isActive: boolean) {
    const name = row.fullName ?? row.name ?? row.email;
    setSaving(true);
    setToast(null);
    try {
      await requestJson<AppUserRow>(`/admin/users/${row.id}/status`, {
        method: "PUT",
        body: JSON.stringify({ isActive }),
      });
      setToast({ type: "success", message: `${name} ${isActive ? "activated" : "deactivated"} successfully.` });
      await loadUsers();
    } catch (err: any) {
      setToast({ type: "error", message: readError(err, "Failed to update user status.") });
    } finally {
      setSaving(false);
    }
  }

  function toggleRole(nextRole: string) {
    setForm((current) => {
      const roles = current.roles.includes(nextRole)
        ? current.roles.filter((item) => item !== nextRole)
        : [...current.roles, nextRole];
      return { ...current, roles: roles.length ? roles : ["User"] };
    });
  }

  return (
    <>
      <PageHeader
        title="Users"
        subtitle="Manage platform members and their roles. Admin only."
        actions={
          <button
            onClick={openInvite}
            disabled={saving}
            className="h-10 px-4 rounded-xl bg-primary text-primary-foreground text-sm font-medium flex items-center gap-2 hover:opacity-90 transition disabled:opacity-60"
          >
            <UserPlus className="size-4" /> Invite user
          </button>
        }
      />

      {toast && (
        <div className={`mb-4 rounded-xl border px-4 py-3 text-sm ${toast.type === "success" ? "border-success/30 bg-success/10 text-success" : "border-destructive/30 bg-destructive/10 text-destructive"}`}>
          {toast.message}
        </div>
      )}

      {error && (
        <div className="mb-4 rounded-xl border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <div className="glass rounded-2xl p-4 mb-4 flex flex-wrap gap-3">
        <input
          value={keyword}
          onChange={(e) => { setKeyword(e.target.value); setPage(1); }}
          placeholder="Search name or email"
          className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm flex-1 min-w-56"
        />
        <select value={role} onChange={(e) => { setRole(e.target.value); setPage(1); }} className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
          <option value="">All roles</option>
          <option>Admin</option>
          <option>Manager</option>
          <option>User</option>
        </select>
        <select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1); }} className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
          <option value="">All statuses</option>
          <option value="active">Active</option>
          <option value="disabled">Disabled</option>
        </select>
        <select value={sortBy} onChange={(e) => setSortBy(e.target.value)} className="h-10 px-3 rounded-lg bg-muted/40 border border-border text-sm">
          <option value="createdAt">Created</option>
          <option value="email">Email</option>
          <option value="fullName">Name</option>
          <option value="role">Role</option>
        </select>
        <button onClick={() => setSortDirection(sortDirection === "asc" ? "desc" : "asc")} className="h-10 px-3 rounded-lg border border-border text-sm">
          {sortDirection === "asc" ? "Asc" : "Desc"}
        </button>
      </div>

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
                const statusLabel = u.isActive ? "Active" : "Disabled";
                const rowRoles = u.roles?.length ? u.roles : [u.role];

                return (
                  <tr key={u.id} className="border-b border-border last:border-0 hover:bg-muted/30 transition">
                    <td className="py-3 px-5">
                      <div className="flex items-center gap-3">
                        <div className="size-8 rounded-lg bg-primary/10 text-primary text-xs font-semibold flex items-center justify-center">
                          {name.split(" ").map((s) => s[0]).join("").slice(0, 2).toUpperCase()}
                        </div>
                        <div className="font-medium">{name}</div>
                      </div>
                    </td>
                    <td className="py-3 pr-4 text-muted-foreground">{u.email}</td>
                    <td className="py-3 pr-4">
                      <div className="flex flex-wrap gap-1">
                        {rowRoles.map((item) => (
                          <span key={item} className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${roleStyle[item] ?? "bg-muted text-muted-foreground"}`}>
                            {item}
                          </span>
                        ))}
                      </div>
                    </td>
                    <td className="py-3 pr-4">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium border ${statusStyle[statusLabel]}`}>
                        {statusLabel}
                      </span>
                    </td>
                    <td className="py-3 pr-4 text-muted-foreground">
                      {u.createdAt ? new Date(u.createdAt).toLocaleDateString() : ""}
                    </td>
                    <td className="py-3 pr-5">
                      <div className="flex items-center gap-1 justify-end">
                        <button
                          onClick={() => openDetails("view", u)}
                          className="size-8 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground flex items-center justify-center transition"
                          title="View"
                        >
                          <Eye className="size-4" />
                        </button>
                        <button
                          onClick={() => openDetails("edit", u)}
                          className="size-8 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground flex items-center justify-center transition"
                          title="Edit"
                        >
                          <Pencil className="size-4" />
                        </button>
                        <button
                          disabled={saving}
                          onClick={() => setUserStatus(u, !u.isActive)}
                          className={`size-8 rounded-lg flex items-center justify-center transition disabled:opacity-50 ${u.isActive ? "hover:bg-destructive/10 text-muted-foreground hover:text-destructive" : "hover:bg-success/10 text-muted-foreground hover:text-success"}`}
                          title={u.isActive ? "Disable" : "Enable"}
                        >
                          {u.isActive ? <Ban className="size-4" /> : <CheckCircle2 className="size-4" />}
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

      <Pagination page={page} totalPages={totalPages} onPage={setPage} />

      {modalMode && (
        <div className="fixed inset-0 z-50 bg-background/70 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="w-full max-w-2xl rounded-2xl border border-border bg-card shadow-[var(--shadow-card)]">
            <div className="px-5 py-4 border-b border-border flex items-center justify-between">
              <div>
                <div className="font-semibold">
                  {modalMode === "invite" ? "Invite user" : modalMode === "edit" ? "Edit user" : "User details"}
                </div>
                <div className="text-xs text-muted-foreground">
                  {modalMode === "invite" ? "Create a local account with assigned roles." : currentName}
                </div>
              </div>
              <button onClick={closeModal} className="size-8 rounded-lg hover:bg-muted flex items-center justify-center">
                <X className="size-4" />
              </button>
            </div>

            <div className="p-5 space-y-4">
              {modalMode === "view" && selectedUser ? (
                <div className="grid sm:grid-cols-2 gap-4 text-sm">
                  <Info label="Name" value={currentName} />
                  <Info label="Email" value={selectedUser.email} />
                  <Info label="Phone" value={selectedUser.phoneNumber || "Not provided"} />
                  <Info label="Status" value={selectedUser.isActive ? "Active" : "Disabled"} />
                  <Info label="Roles" value={(selectedUser.roles?.length ? selectedUser.roles : [selectedUser.role]).join(", ")} />
                  <Info label="Predictions" value={String(selectedUser.predictionCount ?? 0)} />
                  <Info label="Notifications" value={String(selectedUser.notificationCount ?? 0)} />
                  <Info label="Client profile" value={selectedUser.hasClientProfile ? "Completed" : "Not completed"} />
                  <Info label="Created" value={selectedUser.createdAt ? new Date(selectedUser.createdAt).toLocaleString() : ""} />
                  <Info label="Updated" value={selectedUser.updatedAt ? new Date(selectedUser.updatedAt).toLocaleString() : ""} />
                </div>
              ) : (
                <>
                  <div className="grid sm:grid-cols-2 gap-4">
                    <Field label="Full name" value={form.fullName} onChange={(value) => setForm({ ...form, fullName: value })} />
                    <Field label="Email" type="email" value={form.email} onChange={(value) => setForm({ ...form, email: value })} />
                    <Field label="Phone" value={form.phoneNumber} onChange={(value) => setForm({ ...form, phoneNumber: value })} />
                    {modalMode === "invite" && (
                      <Field label="Temporary password" type="password" value={form.password} onChange={(value) => setForm({ ...form, password: value })} placeholder="Leave blank to auto-generate" />
                    )}
                  </div>
                  <div>
                    <div className="text-xs font-medium text-muted-foreground mb-2">Roles</div>
                    <div className="flex flex-wrap gap-2">
                      {availableRoles.map((item) => (
                        <button
                          key={item}
                          type="button"
                          onClick={() => toggleRole(item)}
                          className={`h-9 px-3 rounded-lg border text-sm transition ${form.roles.includes(item) ? "border-primary bg-primary/10 text-primary" : "border-border hover:bg-muted"}`}
                        >
                          {item}
                        </button>
                      ))}
                    </div>
                  </div>
                </>
              )}
            </div>

            <div className="px-5 py-4 border-t border-border flex justify-end gap-2">
              <button onClick={closeModal} className="h-10 px-4 rounded-lg border border-border text-sm hover:bg-muted">
                Close
              </button>
              {modalMode !== "view" && (
                <button
                  onClick={modalMode === "invite" ? saveInvite : saveEdit}
                  disabled={saving || !form.fullName || !form.email}
                  className="h-10 px-4 rounded-lg bg-primary text-primary-foreground text-sm font-medium flex items-center gap-2 hover:opacity-90 disabled:opacity-60"
                >
                  <Save className="size-4" /> {saving ? "Saving..." : "Save"}
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
}

async function requestJson<T>(path: string, init: RequestInit = {}) {
  const res = await apiFetch(path, init);
  const data = await res.json().catch(() => null);
  if (!res.ok) throw new Error(data?.error || data?.message || "Request failed.");
  return data as T;
}

function readError(err: unknown, fallback: string) {
  return err instanceof Error ? err.message : fallback;
}

function Field({ label, value, onChange, type = "text", placeholder }: { label: string; value: string; onChange: (value: string) => void; type?: string; placeholder?: string }) {
  return (
    <label className="block text-sm">
      <span className="text-xs font-medium text-muted-foreground">{label}</span>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="mt-1 h-10 w-full px-3 rounded-lg bg-muted/40 border border-border text-sm"
      />
    </label>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl bg-muted/35 border border-border p-3">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="mt-1 font-medium break-words">{value}</div>
    </div>
  );
}

function Pagination({ page, totalPages, onPage }: { page: number; totalPages: number; onPage: (page: number) => void }) {
  return (
    <div className="mt-4 flex justify-end gap-2 text-sm">
      <button disabled={page <= 1} onClick={() => onPage(page - 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Previous</button>
      <span className="h-9 px-3 flex items-center text-muted-foreground">Page {page} of {totalPages}</span>
      <button disabled={page >= totalPages} onClick={() => onPage(page + 1)} className="h-9 px-3 rounded-lg border border-border disabled:opacity-50">Next</button>
    </div>
  );
}
