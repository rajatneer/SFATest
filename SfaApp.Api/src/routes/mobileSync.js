import express from "express";
import { pool } from "../db.js";
import { requireJwt } from "../middleware/auth.js";

export const mobileSyncRouter = express.Router();

mobileSyncRouter.post("/mobile/sync/push", requireJwt, async (req, res, next) => {
  const conn = await pool.getConnection();
  try {
    const { items } = req.body || {};
    if (!Array.isArray(items) || items.length === 0) {
      return res.status(400).json({ error: "items array is required." });
    }

    await conn.beginTransaction();

    for (const item of items) {
      if (!item?.entityType || !item?.entityRef) {
        throw new Error("Invalid sync item payload.");
      }

      await conn.query(
        `INSERT INTO sync_events
          (entity_type, entity_ref, payload_json, status, retry_count, synced_by)
         VALUES (?, ?, ?, 'accepted', 0, ?)`,
        [
          item.entityType,
          item.entityRef,
          JSON.stringify(item.payload || {}),
          req.user.username
        ]
      );
    }

    await conn.commit();

    return res.status(201).json({
      accepted: items.length,
      syncedBy: req.user.username,
      acceptedAtUtc: new Date().toISOString()
    });
  } catch (error) {
    await conn.rollback();
    return next(error);
  } finally {
    conn.release();
  }
});