import express from "express";
import { randomUUID } from "crypto";
import { requireJwt } from "../middleware/auth.js";
import { pool } from "../db.js";
import { config } from "../config.js";
import { putObject } from "../services/objectStorage.js";

export const reportsRouter = express.Router();

function toCsv(rows) {
  const header = "id,entity_type,entity_ref,status,synced_by,created_at";
  const body = rows
    .map((r) => [r.id, r.entity_type, r.entity_ref, r.status, r.synced_by, r.created_at.toISOString()].join(","))
    .join("\n");
  return `${header}\n${body}`;
}

reportsRouter.get("/reports/daily", requireJwt, async (req, res, next) => {
  const conn = await pool.getConnection();
  try {
    const date = req.query.date || new Date().toISOString().slice(0, 10);

    const [rows] = await conn.query(
      `SELECT id, entity_type, entity_ref, status, synced_by, created_at
       FROM sync_events
       WHERE DATE(created_at) = ?
       ORDER BY id DESC`,
      [date]
    );

    const csv = toCsv(rows);
    const fileName = `sync-report-${date}-${randomUUID()}.csv`;
    const storageKey = `reports/${fileName}`;

    await putObject(storageKey, Buffer.from(csv, "utf8"), "text/csv");

    await conn.beginTransaction();
    const [result] = await conn.query(
      `INSERT INTO uploaded_files
        (storage_key, bucket_name, original_name, mime_type, size_bytes, uploaded_by)
       VALUES (?, ?, ?, ?, ?, ?)`,
      [
        storageKey,
        config.storage.bucket,
        fileName,
        "text/csv",
        Buffer.byteLength(csv, "utf8"),
        req.user.username
      ]
    );
    await conn.commit();

    return res.json({
      reportDate: date,
      fileId: result.insertId,
      recordCount: rows.length,
      storageKey
    });
  } catch (error) {
    await conn.rollback();
    return next(error);
  } finally {
    conn.release();
  }
});