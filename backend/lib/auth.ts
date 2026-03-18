import bcrypt from "bcryptjs";
import { SignJWT, jwtVerify } from "jose";

import { env } from "@/lib/env";

const secret = new TextEncoder().encode(env.jwtSecret);

export type AuthTokenPayload = {
  userId: string;
  email: string;
  username: string;
};

export async function hashPassword(password: string) {
  return bcrypt.hash(password, 10);
}

export async function verifyPassword(password: string, hash: string) {
  return bcrypt.compare(password, hash);
}

export async function signAuthToken(payload: AuthTokenPayload) {
  return new SignJWT(payload)
    .setProtectedHeader({ alg: "HS256" })
    .setIssuedAt()
    .setExpirationTime("7d")
    .sign(secret);
}

export async function verifyAuthToken(token: string) {
  const { payload } = await jwtVerify(token, secret);
  return payload as unknown as AuthTokenPayload;
}
