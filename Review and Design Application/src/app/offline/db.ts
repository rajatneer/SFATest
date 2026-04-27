import Dexie, { type Table } from "dexie";
import type { SyncQueueItem } from "../context/AppContext";

export class SfaOfflineDb extends Dexie {
  syncQueue!: Table<SyncQueueItem, string>;

  constructor() {
    super("sfa_offline_db");
    this.version(1).stores({
      syncQueue: "id, status, created_at, entity_type, entity_ref"
    });
  }
}

export const offlineDb = new SfaOfflineDb();
