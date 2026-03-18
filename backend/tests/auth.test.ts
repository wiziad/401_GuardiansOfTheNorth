import { beforeEach, describe, expect, it, vi } from "vitest";

const { prismaMock } = vi.hoisted(() => ({
  prismaMock: {
    user: {
      create: vi.fn(),
      findUnique: vi.fn(),
    },
  },
}));

vi.mock("@/lib/prisma", () => ({
  prisma: prismaMock,
}));

import { POST as login } from "@/app/api/auth/login/route";
import { POST as register } from "@/app/api/auth/register/route";
import { hashPassword, signAuthToken, verifyAuthToken } from "@/lib/auth";
import { jsonRequest } from "@/tests/helpers";

describe("auth", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("signs and verifies JWT payloads", async () => {
    const token = await signAuthToken({
      userId: "user-1",
      email: "player@example.com",
      username: "player1",
    });

    const payload = await verifyAuthToken(token);

    expect(payload.userId).toBe("user-1");
    expect(payload.email).toBe("player@example.com");
  });

  it("registers a new user", async () => {
    prismaMock.user.create.mockResolvedValue({
      id: "user-1",
      email: "player@example.com",
      username: "player1",
      createdAt: new Date("2026-03-17T00:00:00.000Z"),
    });

    const response = await register(
      jsonRequest("http://localhost/api/auth/register", "POST", {
        email: "player@example.com",
        username: "player1",
        password: "secret123",
      }),
    );

    const body = await response.json();

    expect(response.status).toBe(201);
    expect(body.success).toBe(true);
    expect(body.data.user.email).toBe("player@example.com");
    expect(typeof body.data.token).toBe("string");
  });

  it("logs in an existing user", async () => {
    const passwordHash = await hashPassword("secret123");

    prismaMock.user.findUnique.mockResolvedValue({
      id: "user-1",
      email: "player@example.com",
      username: "player1",
      passwordHash,
      createdAt: new Date("2026-03-17T00:00:00.000Z"),
    });

    const response = await login(
      jsonRequest("http://localhost/api/auth/login", "POST", {
        email: "player@example.com",
        password: "secret123",
      }),
    );

    const body = await response.json();

    expect(response.status).toBe(200);
    expect(body.success).toBe(true);
    expect(body.data.user.username).toBe("player1");
    expect(typeof body.data.token).toBe("string");
  });
});
