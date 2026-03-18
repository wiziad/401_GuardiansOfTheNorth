import { describe, expect, it } from "vitest";

import { createSaveSchema, loginSchema, registerSchema } from "@/lib/validators";

describe("validators", () => {
  it("accepts valid registration data", () => {
    const result = registerSchema.safeParse({
      email: "PLAYER@EXAMPLE.COM",
      username: "player1",
      password: "secret123",
    });

    expect(result.success).toBe(true);
    expect(result.success && result.data.email).toBe("player@example.com");
  });

  it("rejects an invalid login payload", () => {
    const result = loginSchema.safeParse({
      email: "bad-email",
      password: "123",
    });

    expect(result.success).toBe(false);
  });

  it("accepts a valid save payload", () => {
    const result = createSaveSchema.safeParse({
      slotName: "Slot 1",
      mapId: "starter-map",
      playerX: 10,
      playerY: 5,
      hp: 100,
      mana: 40,
      level: 2,
    });

    expect(result.success).toBe(true);
  });
});
