import { NextRequest } from "next/server";

export function jsonRequest(
  url: string,
  method: string,
  body?: Record<string, unknown>,
  headers?: Record<string, string>,
) {
  return new NextRequest(url, {
    method,
    headers: {
      "content-type": "application/json",
      ...headers,
    },
    body: body ? JSON.stringify(body) : undefined,
  });
}
