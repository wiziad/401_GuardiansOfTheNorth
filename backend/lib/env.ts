function requireEnv(name: string): string {
  const value = process.env[name];

  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }

  return value;
}

function getJwtSecret(): string {
  if (process.env.JWT_SECRET) {
    return process.env.JWT_SECRET;
  }

  const fallback = "dev-insecure-jwt-secret-change-me";

  if (!globalThis.__jwtSecretWarningShown) {
    console.warn(
      "[auth] JWT_SECRET is not set. Falling back to a default secret. Set JWT_SECRET in Render before real deployment.",
    );
    globalThis.__jwtSecretWarningShown = true;
  }

  return fallback;
}

function getBooleanEnv(name: string, fallback: boolean) {
  const value = process.env[name];

  if (value === undefined) {
    return fallback;
  }

  return value.toLowerCase() === "true";
}

function getNumberEnv(name: string, fallback: number) {
  const value = process.env[name];
  const parsed = Number(value);

  if (!value || Number.isNaN(parsed) || parsed <= 0) {
    return fallback;
  }

  return parsed;
}

declare global {
  // eslint-disable-next-line no-var
  var __jwtSecretWarningShown: boolean | undefined;
}

export const env = {
  databaseUrl: requireEnv("DATABASE_URL"),
  jwtSecret: getJwtSecret(),
  corsOrigin: process.env.CORS_ORIGIN ?? "*",
  enableDbKeepAlive: getBooleanEnv("ENABLE_DB_KEEP_ALIVE", true),
  keepAliveIntervalMinutes: getNumberEnv("KEEP_ALIVE_INTERVAL_MINUTES", 30),
};
