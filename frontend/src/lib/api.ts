export const API_BASE_URL = "http://localhost:5086/api";

export const TOKEN_KEY = "creditiq:token";
export const USER_STORAGE_KEY = "creditiq:user";

export function getAuthToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function clearAuthStorage() {
  localStorage.removeItem(TOKEN_KEY);
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

  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
  });

  if (res.status === 401) {
    clearAuthStorage();
  }

  return res;
}
