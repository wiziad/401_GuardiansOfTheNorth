import { prisma } from "@/lib/prisma";
import { env } from "@/lib/env";

declare global {
  // eslint-disable-next-line no-var
  var __dbKeepAliveTimer: NodeJS.Timeout | undefined;
}

async function pingDatabase() {
  try {
    await prisma.$queryRawUnsafe("SELECT 1");
    console.log("[db-keep-alive] ping ok");
  } catch (error) {
    console.error("[db-keep-alive] ping failed", error);
  }
}

export function startDatabaseKeepAlive() {
  if (!env.enableDbKeepAlive || process.env.NODE_ENV === "test") {
    return;
  }

  if (global.__dbKeepAliveTimer) {
    return;
  }

  const intervalMs = env.keepAliveIntervalMinutes * 60 * 1000;

  void pingDatabase();
  global.__dbKeepAliveTimer = setInterval(() => {
    void pingDatabase();
  }, intervalMs);
}
