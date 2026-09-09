/**
 * Liveness endpoint for container orchestration (see the Dockerfile's HEALTHCHECK
 * and docker-compose.yml). Intentionally does not check backend/API connectivity —
 * this only answers "is the Next.js server process itself alive and serving
 * requests", which is what a container HEALTHCHECK is meant to verify.
 */
export async function GET() {
  return Response.json({ status: "ok" });
}
