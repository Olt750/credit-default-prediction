import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  ReactNode,
} from "react";

import {
  API_BASE_URL,
  TOKEN_KEY,
  USER_STORAGE_KEY,
  apiFetch,
  clearAuthStorage,
} from "./api";

export type Role = "Admin" | "User";

export type AuthUser = {
  name: string;
  email: string;
  role: Role;
};

type AuthContextValue = {
  user: AuthUser | null;
  ready: boolean;
  login: (
    email: string,
    password: string
  ) => Promise<{ ok: true } | { ok: false; error: string }>;
  register: (data: {
    fullName: string;
    email: string;
    password: string;
    confirmPassword: string;
    phoneNumber: string;
  }) => Promise<{ ok: true } | { ok: false; error: string }>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const tryLoad = async () => {
      try {
        const token = localStorage.getItem(TOKEN_KEY);

        if (token) {
          const res = await apiFetch("/auth/me");

          if (res.ok) {
            const data = await res.json();

            const safeUser: AuthUser = {
              name: data.user?.fullName ?? data.fullName,
              email: data.user?.email ?? data.email,
              role: data.user?.role ?? data.role,
            };

            setUser(safeUser);
            localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(safeUser));
          } else {
            clearAuthStorage();
            setUser(null);
          }
        } else {
          clearAuthStorage();
          setUser(null);
        }
      } catch (error) {
        console.error("Auth initialization error:", error);
        clearAuthStorage();
        setUser(null);
      } finally {
        setReady(true);
      }
    };

    tryLoad();
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      ready,

      login: async (email, password) => {
        try {
          const res = await fetch(`${API_BASE_URL}/auth/login`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              email,
              password,
            }),
          });

          const data = await res.json();

          if (!res.ok) {
            return {
              ok: false,
              error:
                data?.message ||
                data?.error ||
                "Invalid email or password.",
            };
          }

          localStorage.setItem(TOKEN_KEY, data.token);

          const safeUser: AuthUser = {
            name: data.user?.fullName ?? data.fullName,
            email: data.user?.email ?? email,
            role: data.user?.role ?? "User",
          };

          localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(safeUser));

          setUser(safeUser);

          return { ok: true };
        } catch (error) {
          console.error(error);

          return {
            ok: false,
            error: "Network error.",
          };
        }
      },

      register: async (formData) => {
        try {
          const res = await fetch(`${API_BASE_URL}/auth/register`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify(formData),
          });

          const data = await res.json();

          if (!res.ok) {
            return {
              ok: false,
              error:
                data?.message ||
                data?.error ||
                "Registration failed.",
            };
          }

          return { ok: true };
        } catch (error) {
          console.error(error);

          return {
            ok: false,
            error: "Network error.",
          };
        }
      },

      logout: () => {
        clearAuthStorage();
        setUser(null);
      },
    }),
    [user, ready]
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error(
      "useAuth must be used within AuthProvider"
    );
  }

  return context;
}
