import express from "express";
import { pool } from "../db.js";
import { createCacheClient } from "../services/cache.js";

export const healthRouter = express.Router();

healthRouter.get("/health", async (req, res, next) => {
  try {
    const health = {
      status: "ok",
      mysql: "down",
      redis: "disabled",
      timestampUtc: new Date().toISOString()
    };

    await pool.query("SELECT 1");
    health.mysql = "up";

    const redisClient = createCacheClient();
    if (redisClient) {
      try {
        if (redisClient.status !== "ready") {
          await redisClient.connect();
        }
        await redisClient.ping();
        health.redis = "up";
      } catch {
        health.redis = "down";
      }
    }

    return res.json(health);
  } catch (error) {
    return next(error);
  }
});