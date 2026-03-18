import { NextRequest } from "next/server";

import { fail } from "@/lib/response";
import { verifyAuthToken } from "@/lib/auth";

export async function requireUser(request: NextRequest) {
  const authHeader = request.headers.get("authorization");

  if (!authHeader?.startsWith("Bearer ")) {
    return { error: fail("Missing Bearer token", 401) };
  }

  const token = authHeader.slice("Bearer ".length);

  try {
    const payload = await verifyAuthToken(token);
    return { user: payload };
  } catch {
    return { error: fail("Invalid or expired token", 401) };
  }
}
