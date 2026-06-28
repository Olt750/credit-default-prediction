import { apiFetch } from "@/lib/api";

export type NotificationItem = {
  id: string;
  userId: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  readAt?: string | null;
  createdAt: string;
  updatedAt: string;
};

export async function getNotifications(page = 1, pageSize = 20) {
  const res = await apiFetch(`/notifications?page=${page}&pageSize=${pageSize}`);
  if (!res.ok) throw new Error("Failed to load notifications.");
  return (await res.json()) as NotificationItem[];
}

export async function getUnreadNotificationCount() {
  const res = await apiFetch("/notifications/unread-count");
  if (!res.ok) throw new Error("Failed to load unread count.");
  const data = await res.json();
  return Number(data.count ?? 0);
}

export async function markNotificationRead(id: string) {
  const res = await apiFetch(`/notifications/${id}/read`, { method: "PUT" });
  if (!res.ok) throw new Error("Failed to mark notification as read.");
}

export async function markAllNotificationsRead() {
  const res = await apiFetch("/notifications/read-all", { method: "PUT" });
  if (!res.ok) throw new Error("Failed to mark notifications as read.");
}
