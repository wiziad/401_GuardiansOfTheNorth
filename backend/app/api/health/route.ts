import { prisma } from "@/lib/prisma";
import { fail, ok, optionsResponse } from "@/lib/response";

export function OPTIONS() {
  return optionsResponse();
}

export async function GET() {
  try {
    await prisma.$queryRawUnsafe("SELECT 1");

    return ok({
      status: "ok",
      database: "reachable",
      timestamp: new Date().toISOString(),
    });
  } catch {
    return fail("Database health check failed", 500);
  }
}
