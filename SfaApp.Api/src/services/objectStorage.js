import fs from "fs";
import path from "path";
import { S3Client, PutObjectCommand, GetObjectCommand } from "@aws-sdk/client-s3";
import { config } from "../config.js";

let s3Client = null;

function getS3Client() {
  if (s3Client) {
    return s3Client;
  }

  s3Client = new S3Client({
    region: config.storage.s3.region,
    endpoint: config.storage.s3.endpoint,
    forcePathStyle: config.storage.s3.forcePathStyle,
    credentials: {
      accessKeyId: config.storage.s3.accessKeyId,
      secretAccessKey: config.storage.s3.secretAccessKey
    }
  });

  return s3Client;
}

export async function putObject(storageKey, dataBuffer, contentType) {
  try {
    if (config.storage.driver === "s3") {
      const client = getS3Client();
      const command = new PutObjectCommand({
        Bucket: config.storage.bucket,
        Key: storageKey,
        Body: dataBuffer,
        ContentType: contentType
      });
      await client.send(command);
      return;
    }

    const filePath = path.join(config.storage.localPath, storageKey);
    await fs.promises.mkdir(path.dirname(filePath), { recursive: true });
    await fs.promises.writeFile(filePath, dataBuffer);
  } catch (error) {
    console.error("Object storage write failed", error);
    throw error;
  }
}

export async function getObjectReadStream(storageKey) {
  try {
    if (config.storage.driver === "s3") {
      const client = getS3Client();
      const command = new GetObjectCommand({
        Bucket: config.storage.bucket,
        Key: storageKey
      });
      const result = await client.send(command);
      return result.Body;
    }

    const filePath = path.join(config.storage.localPath, storageKey);
    return fs.createReadStream(filePath);
  } catch (error) {
    console.error("Object storage read failed", error);
    throw error;
  }
}