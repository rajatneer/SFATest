import express from "express";
import cors from "cors";
import helmet from "helmet";
import morgan from "morgan";
import { config } from "./config.js";
import { ensureSchemaAndSeed } from "./db.js";
import { errorHandler } from "./middleware/errorHandler.js";
import { authRouter } from "./routes/auth.js";
import { healthRouter } from "./routes/health.js";
import { mobileSyncRouter } from "./routes/mobileSync.js";
import { filesRouter } from "./routes/files.js";
import { reportsRouter } from "./routes/reports.js";

const app = express();

app.use(helmet());
app.use(cors());
app.use(express.json({ limit: "5mb" }));
app.use(morgan("dev"));

app.use("/api", healthRouter);
app.use("/api/auth", authRouter);
app.use("/api", mobileSyncRouter);
app.use("/api", filesRouter);
app.use("/api", reportsRouter);

app.use(errorHandler);

async function bootstrap() {
  try {
    await ensureSchemaAndSeed();
    app.listen(config.app.port, () => {
      console.log(`SfaApp.Api running on port ${config.app.port}`);
    });
  } catch (error) {
    console.error("Failed to start API service", error);
    process.exit(1);
  }
}

bootstrap();