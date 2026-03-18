import { startDatabaseKeepAlive } from "@/lib/keep-alive";

export async function register() {
  startDatabaseKeepAlive();
}
