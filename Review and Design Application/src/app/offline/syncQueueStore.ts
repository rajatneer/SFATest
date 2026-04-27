import type { SyncQueueItem } from "../context/AppContext";
import { offlineDb } from "./db";

export async function loadSyncQueueItems(): Promise<SyncQueueItem[]> {
  const rows = await offlineDb.syncQueue.toArray();
  return rows.sort((a, b) => a.created_at.localeCompare(b.created_at));
}

export async function upsertSyncQueueItems(items: SyncQueueItem[]): Promise<void> {
  if (items.length === 0) {
    return;
  }

  await offlineDb.syncQueue.bulkPut(items);
}

export async function clearSyncQueue(): Promise<void> {
  await offlineDb.syncQueue.clear();
}
