import path from "path";
import dotenv from "dotenv";

dotenv.config();

const toNumber = (value, fallback) => {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : fallback;
};

export const config = {
  app: {
    port: toNumber(process.env.PORT, 8080),
    env: process.env.NODE_ENV || "development"
  },
  mysql: {
    host: process.env.MYSQL_HOST || "127.0.0.1",
    port: toNumber(process.env.MYSQL_PORT, 3306),
    database: process.env.MYSQL_DATABASE || "sfa_app",
    user: process.env.MYSQL_USER || "root",
    password: process.env.MYSQL_PASSWORD || "",
    connectionLimit: toNumber(process.env.MYSQL_CONNECTION_LIMIT, 10)
  },
  jwt: {
    secret: process.env.JWT_SECRET || "replace-me-in-env",
    expiresIn: process.env.JWT_EXPIRES_IN || "8h"
  },
  storage: {
    driver: process.env.OBJECT_STORAGE_DRIVER || "local",
    bucket: process.env.OBJECT_STORAGE_BUCKET || "sfa-files",
    localPath: path.resolve(process.cwd(), process.env.LOCAL_OBJECT_STORAGE_PATH || "./storage"),
    s3: {
      endpoint: process.env.S3_ENDPOINT || undefined,
      region: process.env.S3_REGION || "us-east-1",
      accessKeyId: process.env.S3_ACCESS_KEY_ID || "",
      secretAccessKey: process.env.S3_SECRET_ACCESS_KEY || "",
      forcePathStyle: (process.env.S3_FORCE_PATH_STYLE || "true").toLowerCase() === "true"
    }
  },
  redis: {
    url: process.env.REDIS_URL || ""
  }
};