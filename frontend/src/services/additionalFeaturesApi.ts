import { apiFetch, API_BASE_URL, getAuthToken } from "@/lib/api";

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type SearchParams = Record<string, string | number | boolean | undefined | null>;

export function toQuery(params: SearchParams) {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") query.set(key, String(value));
  });
  return query.toString();
}

export async function searchList<T>(name: string, params: SearchParams) {
  const res = await apiFetch(`/search/${name}?${toQuery(params)}`);
  if (!res.ok) throw new Error(`Failed to search ${name}.`);
  return (await res.json()) as PagedResult<T>;
}

export async function getReports(params: SearchParams) {
  const res = await apiFetch(`/reports?${toQuery(params)}`);
  if (!res.ok) throw new Error("Failed to load reports.");
  return await res.json();
}

export async function generateReport(body: Record<string, unknown>) {
  const res = await apiFetch("/reports/generate", {
    method: "POST",
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const data = await res.json().catch(() => null);
    throw new Error(data?.error || "Failed to generate report.");
  }
  return await res.json();
}

export function downloadUrl(path: string) {
  const token = getAuthToken();
  return `${API_BASE_URL}${path}${path.includes("?") ? "&" : "?"}access_token_hint=${token ?? ""}`;
}

export async function downloadApiFile(path: string) {
  const res = await apiFetch(path);
  if (!res.ok) throw new Error("Download failed.");
  const blob = await res.blob();
  const disposition = res.headers.get("content-disposition") || "";
  const match = disposition.match(/filename\*?=(?:UTF-8'')?"?([^";]+)"?/i);
  const fileName = decodeURIComponent(match?.[1] || "download");
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

export async function importData(dataType: string, file: File) {
  const form = new FormData();
  form.append("file", file);
  const res = await apiFetch(`/import/${dataType}`, {
    method: "POST",
    body: form,
  });
  const data = await res.json().catch(() => null);
  if (!res.ok) throw new Error(data?.error || "Import failed.");
  return data;
}
