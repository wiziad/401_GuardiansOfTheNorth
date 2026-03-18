import { NextRequest } from "next/server";

import { requireUser } from "@/lib/current-user";
import { prisma } from "@/lib/prisma";
import { fail, ok, optionsResponse } from "@/lib/response";

export function OPTIONS() {
  return optionsResponse();
}

export async function GET(request: NextRequest) {
  const auth = await requireUser(request);

  if (auth.error) {
    return auth.error;
  }

  const user = await prisma.user.findUnique({
    where: { id: auth.user.userId },
    select: {
      id: true,
      email: true,
      username: true,
      createdAt: true,
    },
  });

  if (!user) {
    return fail("User not found", 404);
  }

  return ok(user);
}
