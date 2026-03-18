import { NextRequest } from "next/server";

import { signAuthToken, verifyPassword } from "@/lib/auth";
import { prisma } from "@/lib/prisma";
import { fail, ok, optionsResponse } from "@/lib/response";
import { loginSchema } from "@/lib/validators";

export function OPTIONS() {
  return optionsResponse();
}

export async function POST(request: NextRequest) {
  const body = await request.json().catch(() => null);
  const parsed = loginSchema.safeParse(body);

  if (!parsed.success) {
    return fail(parsed.error.issues[0]?.message ?? "Invalid request body", 400);
  }

  const { email, password } = parsed.data;
  const user = await prisma.user.findUnique({
    where: { email },
  });

  if (!user) {
    return fail("Invalid email or password", 401);
  }

  const matches = await verifyPassword(password, user.passwordHash);

  if (!matches) {
    return fail("Invalid email or password", 401);
  }

  const token = await signAuthToken({
    userId: user.id,
    email: user.email,
    username: user.username,
  });

  return ok({
    token,
    user: {
      id: user.id,
      email: user.email,
      username: user.username,
      createdAt: user.createdAt,
    },
  });
}
