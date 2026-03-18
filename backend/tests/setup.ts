import { config } from "dotenv";

config({ path: ".env" });

process.env.DATABASE_URL =
  process.env.DATABASE_URL ??
  "postgresql://test:test@localhost:5432/guardians_test";
process.env.JWT_SECRET = process.env.JWT_SECRET ?? "test-secret";
process.env.CORS_ORIGIN = process.env.CORS_ORIGIN ?? "*";
