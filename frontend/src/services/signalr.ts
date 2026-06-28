import * as signalR from "@microsoft/signalr";
import { API_BASE_URL, getAuthToken } from "@/lib/api";
import type { NotificationItem } from "./notificationsApi";

const HUB_URL = API_BASE_URL.replace(/\/api$/, "/hubs/notifications");

export type NotificationHandlers = {
  onNotificationReceived?: (notification: NotificationItem) => void;
  onUnreadCountUpdated?: (count: number) => void;
  onPredictionCompleted?: (payload: unknown) => void;
  onHighRiskAlert?: (payload: unknown) => void;
};

export function createNotificationsConnection(handlers: NotificationHandlers) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => getAuthToken() ?? "",
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  connection.on("NotificationReceived", (notification: NotificationItem) => {
    handlers.onNotificationReceived?.(notification);
  });

  connection.on("UnreadCountUpdated", (count: number) => {
    handlers.onUnreadCountUpdated?.(Number(count ?? 0));
  });

  connection.on("PredictionCompleted", (payload: unknown) => {
    handlers.onPredictionCompleted?.(payload);
  });

  connection.on("HighRiskAlert", (payload: unknown) => {
    handlers.onHighRiskAlert?.(payload);
  });

  connection.on("SettingsChanged", (payload: unknown) => {
    handlers.onHighRiskAlert?.(payload);
  });

  return connection;
}
