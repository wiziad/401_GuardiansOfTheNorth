import { NextResponse } from "next/server";

import { env } from "@/lib/env";

type JsonPayload = Record<string, unknown>;

function corsHeaders() {
  return {
    "Access-Control-Allow-Origin": env.corsOrigin,
    "Access-Control-Allow-Methods": "GET,POST,PUT,OPTIONS",
    "Access-Control-Allow-Headers": "Content-Type, Authorization",
  };
}

export function ok(data: JsonPayload | JsonPayload[] | null, status = 200) {
  return NextResponse.json(
    { success: true, data },
    {
      status,
      headers: corsHeaders(),
    },
  );
}

export function fail(message: string, status = 400) {
  return NextResponse.json(
    { success: false, message },
    {
      status,
      headers: corsHeaders(),
    },
  );
}

export function optionsResponse() {
  return new NextResponse(null, {
    status: 204,
    headers: corsHeaders(),
  });
}
