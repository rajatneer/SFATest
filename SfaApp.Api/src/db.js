import mysql from "mysql2/promise";
import bcrypt from "bcryptjs";
import { config } from "./config.js";

export const pool = mysql.createPool({
  host: config.mysql.host,
  port: config.mysql.port,
  database: config.mysql.database,
  user: config.mysql.user,
  password: config.mysql.password,
  waitForConnections: true,
  connectionLimit: config.mysql.connectionLimit,
  queueLimit: 0,
  timezone: "Z"
});

export async function ensureSchemaAndSeed() {
  const conn = await pool.getConnection();
  try {
    await conn.beginTransaction();

    await conn.query(`
      CREATE TABLE IF NOT EXISTS api_users (
        id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
        username VARCHAR(120) NOT NULL UNIQUE,
        password_hash VARCHAR(255) NOT NULL,
        role VARCHAR(50) NOT NULL,
        is_active TINYINT(1) NOT NULL DEFAULT 1,
        created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
      ) ENGINE=InnoDB;
    `);

    await conn.query(`
      CREATE TABLE IF NOT EXISTS sync_events (
        id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
        entity_type VARCHAR(50) NOT NULL,
        entity_ref VARCHAR(100) NOT NULL,
        payload_json JSON NOT NULL,
        status VARCHAR(30) NOT NULL DEFAULT 'accepted',
        retry_count INT NOT NULL DEFAULT 0,
        synced_by VARCHAR(120) NOT NULL,
        created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        INDEX idx_sync_events_created_at (created_at),
        INDEX idx_sync_events_entity_ref (entity_ref)
      ) ENGINE=InnoDB;
    `);

    await conn.query(`
      CREATE TABLE IF NOT EXISTS uploaded_files (
        id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
        storage_key VARCHAR(260) NOT NULL,
        bucket_name VARCHAR(120) NOT NULL,
        original_name VARCHAR(255) NOT NULL,
        mime_type VARCHAR(120) NOT NULL,
        size_bytes BIGINT NOT NULL,
        uploaded_by VARCHAR(120) NOT NULL,
        created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        INDEX idx_uploaded_files_created_at (created_at)
      ) ENGINE=InnoDB;
    `);

    const [rows] = await conn.query("SELECT id FROM api_users WHERE username = ? LIMIT 1", ["apiadmin"]);
    if (rows.length === 0) {
      const hashed = await bcrypt.hash("Admin@12345", 10);
      await conn.query(
        "INSERT INTO api_users (username, password_hash, role, is_active) VALUES (?, ?, ?, 1)",
        ["apiadmin", hashed, "Admin"]
      );
    }

    await conn.commit();
  } catch (error) {
    await conn.rollback();
    throw error;
  } finally {
    conn.release();
  }
}