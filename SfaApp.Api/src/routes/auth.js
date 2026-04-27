import express from "express";
import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";
import { pool } from "../db.js";
import { config } from "../config.js";

export const authRouter = express.Router();

authRouter.post("/login", async (req, res, next) => {
  try {
    const { username, password } = req.body || {};
    if (!username || !password) {
      return res.status(400).json({ error: "username and password are required." });
    }

    const [rows] = await pool.query(
      "SELECT id, username, password_hash, role, is_active FROM api_users WHERE username = ? LIMIT 1",
      [username]
    );

    if (!rows.length || rows[0].is_active !== 1) {
      return res.status(401).json({ error: "Invalid credentials." });
    }

    const user = rows[0];
    const isValid = await bcrypt.compare(password, user.password_hash);
    if (!isValid) {
      return res.status(401).json({ error: "Invalid credentials." });
    }

    const token = jwt.sign(
      { sub: String(user.id), username: user.username, role: user.role },
      config.jwt.secret,
      { expiresIn: config.jwt.expiresIn }
    );

    return res.json({
      accessToken: token,
      tokenType: "Bearer",
      expiresIn: config.jwt.expiresIn,
      user: {
        id: user.id,
        username: user.username,
        role: user.role
      }
    });
  } catch (error) {
    return next(error);
  }
});