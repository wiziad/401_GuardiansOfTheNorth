import { prisma } from "@/lib/prisma";

interface HealthData {
  status: string;
  database: string;
  timestamp: string;
}

async function getHealthStatus() {
  try {
    await prisma.$queryRawUnsafe("SELECT 1");
    return {
      status: "ok",
      database: "reachable",
      timestamp: new Date().toISOString(),
    };
  } catch {
    return null;
  }
}

async function getDbStats() {
  try {
    const [userCount, saveCount] = await Promise.all([
      prisma.user.count(),
      prisma.saveSlot.count(),
    ]);
    return { userCount, saveCount, reachable: true };
  } catch {
    return { userCount: 0, saveCount: 0, reachable: false };
  }
}

function formatTimestamp(iso: string): string {
  try {
    const d = new Date(iso);
    return d.toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
    });
  } catch {
    return iso;
  }
}

export default async function StatusPage() {
  const [health, dbStats] = await Promise.all([
    getHealthStatus(),
    getDbStats(),
  ]);

  const isHealthy = health !== null && dbStats.reachable;

  return (
    <div
      style={{
        minHeight: "100vh",
        background: "#0d0d0d",
        color: "#e8e8e8",
        fontFamily: "'Courier New', Courier, monospace",
        padding: "48px 24px",
        boxSizing: "border-box",
      }}
    >
      {/* Header */}
      <div style={{ maxWidth: 800, margin: "0 auto" }}>
        <div style={{ marginBottom: 8 }}>
          <span style={{ color: "#555", fontSize: 11, letterSpacing: 3 }}>
            ▸ SYSTEM STATUS
          </span>
        </div>
        <h1
          style={{
            margin: "0 0 8px",
            fontSize: 28,
            fontWeight: 700,
            letterSpacing: -0.5,
            color: "#f0f0f0",
          }}
        >
          Guardians Of The North
        </h1>
        <p style={{ margin: 0, color: "#666", fontSize: 13 }}>
          Backend API — PostgreSQL + Next.js + Prisma
        </p>

        {/* Overall Status Badge */}
        <div style={{ marginTop: 32 }}>
          <div
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: 10,
              padding: "10px 20px",
              borderRadius: 6,
              background: isHealthy ? "#0a2a1a" : "#2a0a0a",
              border: `1px solid ${isHealthy ? "#1a5c35" : "#5c1a1a"}`,
            }}
          >
            <div
              style={{
                width: 10,
                height: 10,
                borderRadius: "50%",
                background: isHealthy ? "#22c55e" : "#ef4444",
                boxShadow: isHealthy
                  ? "0 0 8px #22c55e88"
                  : "0 0 8px #ef444488",
                animation: isHealthy ? "pulse 2s infinite" : "none",
              }}
            />
            <span
              style={{
                color: isHealthy ? "#22c55e" : "#ef4444",
                fontWeight: 700,
                fontSize: 14,
                letterSpacing: 2,
              }}
            >
              {isHealthy ? "ALL SYSTEMS OPERATIONAL" : "SYSTEM DEGRADED"}
            </span>
          </div>
        </div>

        {/* Cards Grid */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
            gap: 16,
            marginTop: 32,
          }}
        >
          {/* Service Card */}
          <div
            style={{
              background: "#141414",
              border: "1px solid #2a2a2a",
              borderRadius: 8,
              padding: 24,
            }}
          >
            <div style={{ fontSize: 11, color: "#555", letterSpacing: 2, marginBottom: 12 }}>
              SERVICE
            </div>
            <div style={{ fontSize: 24, fontWeight: 700, color: "#f0f0f0", marginBottom: 4 }}>
              {health ? "ONLINE" : "OFFLINE"}
            </div>
            <div style={{ fontSize: 12, color: "#555" }}>
              {health
                ? `Last checked: ${formatTimestamp(health.timestamp)}`
                : "Service unreachable"}
            </div>
          </div>

          {/* Database Card */}
          <div
            style={{
              background: "#141414",
              border: "1px solid #2a2a2a",
              borderRadius: 8,
              padding: 24,
            }}
          >
            <div style={{ fontSize: 11, color: "#555", letterSpacing: 2, marginBottom: 12 }}>
              DATABASE
            </div>
            <div style={{ fontSize: 24, fontWeight: 700, color: "#f0f0f0", marginBottom: 4 }}>
              {dbStats.reachable ? "CONNECTED" : "DISCONNECTED"}
            </div>
            <div style={{ fontSize: 12, color: "#555" }}>
              {dbStats.reachable
                ? `${dbStats.userCount} users · ${dbStats.saveCount} saves`
                : "Database unreachable"}
            </div>
          </div>

          {/* Runtime Card */}
          <div
            style={{
              background: "#141414",
              border: "1px solid #2a2a2a",
              borderRadius: 8,
              padding: 24,
            }}
          >
            <div style={{ fontSize: 11, color: "#555", letterSpacing: 2, marginBottom: 12 }}>
              RUNTIME
            </div>
            <div style={{ fontSize: 24, fontWeight: 700, color: "#f0f0f0", marginBottom: 4 }}>
              RENDER
            </div>
            <div style={{ fontSize: 12, color: "#555" }}>
              Next.js {health ? "Server" : "—"}
            </div>
          </div>
        </div>

        {/* API Endpoints */}
        <div style={{ marginTop: 40 }}>
          <div style={{ fontSize: 11, color: "#555", letterSpacing: 2, marginBottom: 16 }}>
            ▸ API ENDPOINTS
          </div>
          <div
            style={{
              background: "#141414",
              border: "1px solid #2a2a2a",
              borderRadius: 8,
              overflow: "hidden",
            }}
          >
            {[
              { method: "GET",  path: "/api/health",        desc: "Health check", public: true },
              { method: "GET",  path: "/api/status",        desc: "This page", public: true },
              { method: "POST", path: "/api/auth/register", desc: "Register user", public: true },
              { method: "POST", path: "/api/auth/login",    desc: "Login", public: true },
              { method: "GET",  path: "/api/auth/me",       desc: "Current user (auth)", public: false },
              { method: "GET",  path: "/api/saves",         desc: "List saves (auth)", public: false },
              { method: "POST", path: "/api/saves",         desc: "Create save (auth)", public: false },
              { method: "GET",  path: "/api/saves/:id",     desc: "Get save (auth)", public: false },
              { method: "PUT",  path: "/api/saves/:id",     desc: "Update save (auth)", public: false },
            ].map((endpoint, i) => (
              <div
                key={endpoint.path}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 16,
                  padding: "12px 20px",
                  borderBottom: i < 8 ? "1px solid #1e1e1e" : "none",
                }}
              >
                <span
                  style={{
                    fontSize: 10,
                    fontWeight: 700,
                    padding: "3px 8px",
                    borderRadius: 4,
                    background:
                      endpoint.method === "GET"
                        ? "#0a2040"
                        : endpoint.method === "POST"
                          ? "#0a3020"
                          : "#1a1a40",
                    color:
                      endpoint.method === "GET"
                        ? "#60a5fa"
                        : endpoint.method === "POST"
                          ? "#34d399"
                          : "#a78bfa",
                    minWidth: 48,
                    textAlign: "center",
                    fontFamily: "monospace",
                  }}
                >
                  {endpoint.method}
                </span>
                <span
                  style={{
                    fontFamily: "monospace",
                    fontSize: 13,
                    color: endpoint.public ? "#888" : "#555",
                    flex: 1,
                  }}
                >
                  {endpoint.path}
                </span>
                <span style={{ fontSize: 12, color: "#444" }}>{endpoint.desc}</span>
                <span
                  style={{
                    fontSize: 10,
                    padding: "2px 6px",
                    borderRadius: 3,
                    background: endpoint.public ? "#1a2a1a" : "#2a2a1a",
                    color: endpoint.public ? "#4ade80" : "#facc15",
                  }}
                >
                  {endpoint.public ? "public" : "auth"}
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Footer */}
        <div
          style={{
            marginTop: 48,
            paddingTop: 24,
            borderTop: "1px solid #1e1e1e",
            fontSize: 12,
            color: "#333",
            display: "flex",
            justifyContent: "space-between",
          }}
        >
          <span>Guardians Of The North · Backend v1.0</span>
          <span>
            {isHealthy && health
              ? `Reachable · ${formatTimestamp(health.timestamp)}`
              : "Unreachable"}
          </span>
        </div>
      </div>

      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.5; }
        }
      `}</style>
    </div>
  );
}
