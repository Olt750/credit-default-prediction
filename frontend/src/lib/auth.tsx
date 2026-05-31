import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";

export type Role = "Admin" | "User";

export type AuthUser = {
  name: string;
  email: string;
  role: Role;
};

type Creds = AuthUser & { password: string };

export const STATIC_USERS: Creds[] = [
  { name: "Standard User", email: "user@credit.com", password: "user123", role: "User" },
  { name: "Admin Account", email: "admin@credit.com", password: "admin123", role: "Admin" },
];

type AuthContextValue = {
  user: AuthUser | null;
  ready: boolean;
  login: (email: string, password: string) => { ok: true } | { ok: false; error: string };
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);
const STORAGE_KEY = "creditiq:user";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) setUser(JSON.parse(raw));
    } catch {}
    setReady(true);
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    ready,
    login: (email, password) => {
      const found = STATIC_USERS.find(
        (u) => u.email.toLowerCase() === email.toLowerCase() && u.password === password,
      );
      if (!found) return { ok: false, error: "Invalid email or password." };
      const safe: AuthUser = { name: found.name, email: found.email, role: found.role };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(safe));
      setUser(safe);
      return { ok: true };
    },
    logout: () => {
      localStorage.removeItem(STORAGE_KEY);
      setUser(null);
    },
  }), [user, ready]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}