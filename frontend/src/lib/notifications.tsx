import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import type { HubConnection } from "@microsoft/signalr";
import { useAuth } from "./auth";
import {
  getNotifications,
  getUnreadNotificationCount,
  markAllNotificationsRead,
  markNotificationRead,
  type NotificationItem,
} from "@/services/notificationsApi";
import { createNotificationsConnection } from "@/services/signalr";

type NotificationsContextValue = {
  notifications: NotificationItem[];
  unreadCount: number;
  loading: boolean;
  refresh: () => Promise<void>;
  markRead: (id: string) => Promise<void>;
  markAllRead: () => Promise<void>;
};

const NotificationsContext = createContext<NotificationsContextValue | null>(null);

export function NotificationsProvider({ children }: { children: ReactNode }) {
  const { user, ready } = useAuth();
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(false);

  const refresh = useCallback(async () => {
    if (!user) return;
    setLoading(true);
    try {
      const [items, count] = await Promise.all([
        getNotifications(),
        getUnreadNotificationCount(),
      ]);
      setNotifications(items);
      setUnreadCount(count);
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    if (ready && user) {
      refresh().catch(() => {
        setNotifications([]);
        setUnreadCount(0);
      });
    }
  }, [ready, user, refresh]);

  useEffect(() => {
    if (!ready || !user) return;

    let disposed = false;
    let connection: HubConnection | null = createNotificationsConnection({
      onNotificationReceived: (notification) => {
        setNotifications((current) => [
          notification,
          ...current.filter((item) => item.id !== notification.id),
        ]);
      },
      onUnreadCountUpdated: setUnreadCount,
      onPredictionCompleted: () => {
        refresh().catch(() => undefined);
      },
      onHighRiskAlert: () => {
        refresh().catch(() => undefined);
      },
    });

    connection.start().catch(() => {
      connection = null;
    });

    return () => {
      disposed = true;
      if (connection && !disposed) return;
      connection?.stop().catch(() => undefined);
    };
  }, [ready, user, refresh]);

  const markRead = useCallback(async (id: string) => {
    await markNotificationRead(id);
    setNotifications((current) =>
      current.map((item) =>
        item.id === id
          ? { ...item, isRead: true, readAt: new Date().toISOString() }
          : item
      )
    );
    setUnreadCount((current) => Math.max(0, current - 1));
  }, []);

  const markAllRead = useCallback(async () => {
    await markAllNotificationsRead();
    setNotifications((current) =>
      current.map((item) => ({ ...item, isRead: true, readAt: item.readAt ?? new Date().toISOString() }))
    );
    setUnreadCount(0);
  }, []);

  const value = useMemo<NotificationsContextValue>(
    () => ({ notifications, unreadCount, loading, refresh, markRead, markAllRead }),
    [notifications, unreadCount, loading, refresh, markRead, markAllRead]
  );

  return (
    <NotificationsContext.Provider value={value}>
      {children}
    </NotificationsContext.Provider>
  );
}

export function useNotifications() {
  const context = useContext(NotificationsContext);
  if (!context) {
    throw new Error("useNotifications must be used within NotificationsProvider");
  }

  return context;
}
