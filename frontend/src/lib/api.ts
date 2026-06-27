export const API_BASE_URL = "http://localhost:5086/api";

export const TOKEN_KEY = "creditiq:token";
export const REFRESH_TOKEN_KEY = "creditiq:refresh-token";
export const USER_STORAGE_KEY = "creditiq:user";

export function getAuthToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function clearAuthStorage() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_STORAGE_KEY);
  sessionStorage.clear();
}

export async function apiFetch(path: string, init: RequestInit = {}) {
  const token = getAuthToken();
  const headers = new Headers(init.headers);

  if (!headers.has("Content-Type") && init.body) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let res = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
  });

  if (res.status === 401 && path !== "/auth/refresh") {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      const retryHeaders = new Headers(init.headers);
      if (!retryHeaders.has("Content-Type") && init.body) {
        retryHeaders.set("Content-Type", "application/json");
      }
      retryHeaders.set("Authorization", `Bearer ${localStorage.getItem(TOKEN_KEY)}`);
      res = await fetch(`${API_BASE_URL}${path}`, {
        ...init,
        headers: retryHeaders,
      });
    }
  }

  if (res.status === 401) {
    clearAuthStorage();
  }

  return res;
}

async function refreshAccessToken() {
  const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
  if (!refreshToken) return false;

  const res = await fetch(`${API_BASE_URL}/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  });

  if (!res.ok) return false;

  const data = await res.json();
  const accessToken = data.accessToken ?? data.token;
  if (!accessToken || !data.refreshToken) return false;

  localStorage.setItem(TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
  return true;
}
