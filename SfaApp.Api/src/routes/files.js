import express from "express";
import multer from "multer";
import { randomUUID } from "crypto";
import { pool } from "../db.js";
import { requireJwt } from "../middleware/auth.js";
import { config } from "../config.js";
import { putObject, getObjectReadStream } from "../services/objectStorage.js";

const upload = multer({ storage: multer.memoryStorage() });
export const filesRouter = express.Router();

filesRouter.post("/files/upload", requireJwt, upload.single("file"), async (req, res, next) => {
  const conn = await pool.getConnection();
  try {
    if (!req.file) {
      return res.status(400).json({ error: "file is required." });
    }

    const folder = req.body?.folder || "uploads";
    const storageKey = `${folder}/${Date.now()}-${randomUUID()}-${req.file.originalname}`;

    await putObject(storageKey, req.file.buffer, req.file.mimetype || "application/octet-stream");

    await conn.beginTransaction();
    const [result] = await conn.query(
      `INSERT INTO uploaded_files
        (storage_key, bucket_name, original_name, mime_type, size_bytes, uploaded_by)
       VALUES (?, ?, ?, ?, ?, ?)`,
      [
        storageKey,
        config.storage.bucket,
        req.file.originalname,
        req.file.mimetype || "application/octet-stream",
        req.file.size,
        req.user.username
      ]
    );
    await conn.commit();

    return res.status(201).json({
      fileId: result.insertId,
      storageKey,
      bucket: config.storage.bucket
    });
  } catch (error) {
    await conn.rollback();
    return next(error);
  } finally {
    conn.release();
  }
});

filesRouter.get("/files/:fileId/download", requireJwt, async (req, res, next) => {
  try {
    const fileId = Number(req.params.fileId);
    if (!Number.isFinite(fileId)) {
      return res.status(400).json({ error: "Invalid file id." });
    }

    const [rows] = await pool.query(
      "SELECT id, storage_key, original_name, mime_type FROM uploaded_files WHERE id = ? LIMIT 1",
      [fileId]
    );

    if (!rows.length) {
      return res.status(404).json({ error: "File not found." });
    }

    const file = rows[0];
    const stream = await getObjectReadStream(file.storage_key);

    res.setHeader("Content-Type", file.mime_type || "application/octet-stream");
    res.setHeader("Content-Disposition", `attachment; filename="${file.original_name}"`);
    stream.pipe(res);
    return undefined;
  } catch (error) {
    return next(error);
  }
});