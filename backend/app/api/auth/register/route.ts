import { Prisma } from "@prisma/client";
import { NextRequest } from "next/server";

import { hashPassword, signAuthToken } from "@/lib/auth";
import { prisma } from "@/lib/prisma";
import { fail, ok, optionsResponse } from "@/lib/response";
import { registerSchema } from "@/lib/validators";

export function OPTIONS() {
  return optionsResponse();
}

export async function POST(request: NextRequest) {
  const body = await request.json().catch(() => null);
  const parsed = registerSchema.safeParse(body);

  if (!parsed.success) {
    return fail(parsed.error.issues[0]?.message ?? "Invalid request body", 400);
  }

  const { email, username, password } = parsed.data;

  try {
    const passwordHash = await hashPassword(password);
    const user = await prisma.user.create({
      data: {
        email,
        username,
        passwordHash,
      },
    });

    const token = await signAuthToken({
      userId: user.id,
      email: user.email,
      username: user.username,
    });

    return ok(
      {
        token,
        user: {
          id: user.id,
          email: user.email,
          username: user.username,
          createdAt: user.createdAt,
        },
      },
      201,
    );
  } catch (error) {
    if (
      error instanceof Prisma.PrismaClientKnownRequestError &&
      error.code === "P2002"
    ) {
      return fail("Email or username already exists", 409);
    }

    return fail("Failed to create account", 500);
  }
}
