import * as signalR from "@microsoft/signalr";
import { getSession } from "./session";

let connection: signalR.HubConnection | null = null;

export function getProgressConnection(): signalR.HubConnection {
  if (connection) {
    return connection;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_API_BASE_URL ?? ""}/hubs/progress`, {
      accessTokenFactory: () => getSession()?.token ?? ""
    })
    .withAutomaticReconnect()
    .build();

  return connection;
}

export async function ensureConnected(): Promise<signalR.HubConnection> {
  const hub = getProgressConnection();

  if (hub.state === signalR.HubConnectionState.Disconnected) {
    await hub.start();
  }

  return hub;
}
