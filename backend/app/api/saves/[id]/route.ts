import { NextRequest } from "next/server";

import { requireUser } from "@/lib/current-user";
import { prisma } from "@/lib/prisma";
import { fail, ok, optionsResponse } from "@/lib/response";
import { updateSaveSchema } from "@/lib/validators";

type RouteContext = {
  params: Promise<{
    id: string;
  }>;
};

export function OPTIONS() {
  return optionsResponse();
}

export async function GET(request: NextRequest, context: RouteContext) {
  const auth = await requireUser(request);

  if (auth.error) {
    return auth.error;
  }

  const { id } = await context.params;
  const save = await prisma.saveSlot.findFirst({
    where: {
      id,
      userId: auth.user.userId,
    },
  });

  if (!save) {
    return fail("Save slot not found", 404);
  }

  return ok(save);
}

export async function PUT(request: NextRequest, context: RouteContext) {
  const auth = await requireUser(request);

  if (auth.error) {
    return auth.error;
  }

  const { id } = await context.params;
  const body = await request.json().catch(() => null);
  const parsed = updateSaveSchema.safeParse(body);

  if (!parsed.success) {
    return fail(parsed.error.issues[0]?.message ?? "Invalid request body", 400);
  }

  const existingSave = await prisma.saveSlot.findFirst({
    where: {
      id,
      userId: auth.user.userId,
    },
  });

  if (!existingSave) {
    return fail("Save slot not found", 404);
  }

  const save = await prisma.saveSlot.update({
    where: { id },
    data: parsed.data,
  });

  return ok(save);
}
