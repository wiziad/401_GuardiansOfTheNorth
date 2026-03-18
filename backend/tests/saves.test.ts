import { beforeEach, describe, expect, it, vi } from "vitest";

const { prismaMock } = vi.hoisted(() => ({
  prismaMock: {
    saveSlot: {
      count: vi.fn(),
      create: vi.fn(),
      findMany: vi.fn(),
      findFirst: vi.fn(),
      update: vi.fn(),
    },
  },
}));

vi.mock("@/lib/prisma", () => ({
  prisma: prismaMock,
}));

import { GET as getSaveById, PUT as updateSaveById } from "@/app/api/saves/[id]/route";
import { GET as listSaves, POST as createSave } from "@/app/api/saves/route";
import { signAuthToken } from "@/lib/auth";
import { jsonRequest } from "@/tests/helpers";

async function authHeader() {
  const token = await signAuthToken({
    userId: "user-1",
    email: "player@example.com",
    username: "player1",
  });

  return {
    authorization: `Bearer ${token}`,
  };
}

describe("saves", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("lists the current user's saves", async () => {
    prismaMock.saveSlot.findMany.mockResolvedValue([
      {
        id: "save-1",
        userId: "user-1",
        slotName: "Slot 1",
        mapId: "starter-map",
        playerX: 0,
        playerY: 0,
        hp: 100,
        mana: 50,
        level: 1,
      },
    ]);

    const response = await listSaves(
      jsonRequest("http://localhost/api/saves", "GET", undefined, await authHeader()),
    );
    const body = await response.json();

    expect(response.status).toBe(200);
    expect(body.data).toHaveLength(1);
    expect(body.data[0].id).toBe("save-1");
  });

  it("creates a new save slot", async () => {
    prismaMock.saveSlot.count.mockResolvedValue(0);
    prismaMock.saveSlot.create.mockResolvedValue({
      id: "save-1",
      userId: "user-1",
      slotName: "Slot 1",
      mapId: "starter-map",
      playerX: 0,
      playerY: 0,
      hp: 100,
      mana: 50,
      level: 1,
    });

    const response = await createSave(
      jsonRequest(
        "http://localhost/api/saves",
        "POST",
        {
          slotName: "Slot 1",
          mapId: "starter-map",
          playerX: 0,
          playerY: 0,
          hp: 100,
          mana: 50,
          level: 1,
        },
        await authHeader(),
      ),
    );
    const body = await response.json();

    expect(response.status).toBe(201);
    expect(body.data.slotName).toBe("Slot 1");
  });

  it("gets one save slot by id", async () => {
    prismaMock.saveSlot.findFirst.mockResolvedValue({
      id: "save-1",
      userId: "user-1",
      slotName: "Slot 1",
      mapId: "starter-map",
      playerX: 0,
      playerY: 0,
      hp: 100,
      mana: 50,
      level: 1,
    });

    const response = await getSaveById(
      jsonRequest("http://localhost/api/saves/save-1", "GET", undefined, await authHeader()),
      { params: Promise.resolve({ id: "save-1" }) },
    );
    const body = await response.json();

    expect(response.status).toBe(200);
    expect(body.data.id).toBe("save-1");
  });

  it("updates one save slot", async () => {
    prismaMock.saveSlot.findFirst.mockResolvedValue({
      id: "save-1",
      userId: "user-1",
      slotName: "Slot 1",
      mapId: "starter-map",
      playerX: 0,
      playerY: 0,
      hp: 100,
      mana: 50,
      level: 1,
    });
    prismaMock.saveSlot.update.mockResolvedValue({
      id: "save-1",
      userId: "user-1",
      slotName: "Slot 1",
      mapId: "starter-map",
      playerX: 20,
      playerY: 8,
      hp: 90,
      mana: 50,
      level: 1,
    });

    const response = await updateSaveById(
      jsonRequest(
        "http://localhost/api/saves/save-1",
        "PUT",
        {
          playerX: 20,
          playerY: 8,
          hp: 90,
        },
        await authHeader(),
      ),
      { params: Promise.resolve({ id: "save-1" }) },
    );
    const body = await response.json();

    expect(response.status).toBe(200);
    expect(body.data.playerX).toBe(20);
    expect(body.data.hp).toBe(90);
  });
});
