import { NextRequest } from "next/server";

import { requireUser } from "@/lib/current-user";
import { prisma } from "@/lib/prisma";
import { fail, ok, optionsResponse } from "@/lib/response";
import { createSaveSchema } from "@/lib/validators";

export function OPTIONS() {
  return optionsResponse();
}

export async function GET(request: NextRequest) {
  const auth = await requireUser(request);

  if (auth.error) {
    return auth.error;
  }

  const saves = await prisma.saveSlot.findMany({
    where: { userId: auth.user.userId },
    orderBy: { updatedAt: "desc" },
  });

  return ok(saves);
}

export async function POST(request: NextRequest) {
  const auth = await requireUser(request);

  if (auth.error) {
    return auth.error;
  }

  const body = await request.json().catch(() => null);
  const parsed = createSaveSchema.safeParse(body);

  if (!parsed.success) {
    return fail(parsed.error.issues[0]?.message ?? "Invalid request body", 400);
  }

  const existingCount = await prisma.saveSlot.count({
    where: { userId: auth.user.userId },
  });

  if (existingCount >= 3) {
    return fail("Each user can only have 3 save slots", 400);
  }

  const save = await prisma.saveSlot.create({
    data: {
      userId: auth.user.userId,
      ...parsed.data,
    },
  });

  return ok(save, 201);
}
