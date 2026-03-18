import { z } from "zod";

export const registerSchema = z.object({
  email: z.string().trim().toLowerCase().email(),
  username: z.string().trim().min(3).max(30),
  password: z.string().min(6).max(100),
});

export const loginSchema = z.object({
  email: z.string().trim().toLowerCase().email(),
  password: z.string().min(6).max(100),
});

export const createSaveSchema = z.object({
  slotName: z.string().trim().min(1).max(40),
  mapId: z.string().trim().min(1).max(50).default("starter-map"),
  playerX: z.number().finite(),
  playerY: z.number().finite(),
  hp: z.number().int().min(0).default(100),
  mana: z.number().int().min(0).default(50),
  level: z.number().int().min(1).default(1),
});

export const updateSaveSchema = createSaveSchema.partial().refine(
  (payload) => Object.keys(payload).length > 0,
  "At least one field must be updated",
);
