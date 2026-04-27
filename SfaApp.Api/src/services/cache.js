import Redis from "ioredis";
import { config } from "../config.js";

let redisClient = null;

export function createCacheClient() {
  if (!config.redis.url) {
    return null;
  }

  if (redisClient) {
    return redisClient;
  }

  redisClient = new Redis(config.redis.url, {
    lazyConnect: true,
    maxRetriesPerRequest: 1
  });

  redisClient.on("error", (error) => {
    console.error("Redis connection error", error);
  });

  return redisClient;
}

export async function getCachedJson(key) {
  const client = createCacheClient();
  if (!client) {
    return null;
  }

  try {
    if (client.status !== "ready") {
      await client.connect();
    }
    const raw = await client.get(key);
    return raw ? JSON.parse(raw) : null;
  } catch (error) {
    console.error("Redis read error", error);
    return null;
  }
}

export async function setCachedJson(key, value, ttlSeconds = 60) {
  const client = createCacheClient();
  if (!client) {
    return;
  }

  try {
    if (client.status !== "ready") {
      await client.connect();
    }
    await client.set(key, JSON.stringify(value), "EX", ttlSeconds);
  } catch (error) {
    console.error("Redis write error", error);
  }
}