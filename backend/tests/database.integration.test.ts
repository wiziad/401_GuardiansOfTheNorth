import { PrismaClient } from "@prisma/client";
import { afterAll, describe, expect, it } from "vitest";

import { hashPassword } from "@/lib/auth";

const databaseUrl = process.env.DATABASE_URL;

const prisma = databaseUrl
  ? new PrismaClient({
      datasourceUrl: databaseUrl,
    })
  : null;

const createdUserIds: string[] = [];

describe("database integration", () => {
  it("connects to the real database and performs user/save CRUD", async () => {
    if (!prisma) {
      expect.skip("DATABASE_URL is not set");
      return;
    }

    const suffix = `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
    const email = `integration-${suffix}@example.com`;
    const username = `integration_${suffix}`.slice(0, 30);
    const passwordHash = await hashPassword("secret123");

    const user = await prisma.user.create({
      data: {
        email,
        username,
        passwordHash,
      },
    });

    createdUserIds.push(user.id);

    const save = await prisma.saveSlot.create({
      data: {
        userId: user.id,
        slotName: "Integration Slot",
        mapId: "starter-map",
        playerX: 12.5,
        playerY: 7.25,
        hp: 95,
        mana: 40,
        level: 2,
      },
    });

    const fetchedUser = await prisma.user.findUnique({
      where: { id: user.id },
      include: {
        saveSlots: true,
      },
    });

    expect(fetchedUser).not.toBeNull();
    expect(fetchedUser?.email).toBe(email);
    expect(fetchedUser?.saveSlots).toHaveLength(1);
    expect(fetchedUser?.saveSlots[0]?.id).toBe(save.id);

    const updatedSave = await prisma.saveSlot.update({
      where: { id: save.id },
      data: {
        hp: 88,
        playerX: 20,
      },
    });

    expect(updatedSave.hp).toBe(88);
    expect(updatedSave.playerX).toBe(20);
  }, 20000);
});

afterAll(async () => {
  if (!prisma) {
    return;
  }

  for (const userId of createdUserIds) {
    await prisma.user.delete({
      where: { id: userId },
    });
  }

  await prisma.$disconnect();
});
