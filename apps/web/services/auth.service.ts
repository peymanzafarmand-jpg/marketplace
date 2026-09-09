import { apiClient } from "@/services/api-client";
import { useAuthStore } from "@/lib/store/auth-store";
import { ApiError } from "@/types/api";
import type { User } from "@/types/domain";

/**
 * Auth service — Task 1.6 (Frontend Auth Storage Correction).
 *
 * The access token lives only in memory (see `lib/store/auth-store.ts`),
 * so it does not survive a full page reload / new tab / browser restart.
 * `bootstrapSession()` is how the app recovers a session after that: it
 * calls the refresh endpoint, which reads the refresh token from an
 * HttpOnly cookie (never exposed to JavaScript, so not readable or
 * exfiltratable via an XSS payload) and returns a fresh access token.
 *
 * WIRING (left for the Auth UI feature sprint, out of scope here):
 * call `bootstrapSession()` once, as early as possible, from a client
 * root component (e.g. a `<SessionBootstrap />` mounted in
 * `app/layout.tsx` or `PublicLayout`) so the in-memory token is restored
 * before any authenticated request fires. Until that root component is
 * added, this function is implemented and exported but not yet called
 * anywhere in the app.
 */

interface RefreshTokenResponse {
  user: User;
  accessToken: string;
}

/**
 * Attempts to restore a session on app start.
 *
 * Expected backend contract (Task 1.6 — depends on Backend Sprint 1,
 * section 11): `POST /api/v1/auth/refresh-token`
 *   - Reads the refresh token from an HttpOnly cookie set at login —
 *     never from the request body, and never accessible to JS.
 *   - On success (200): returns `{ user, accessToken }`; this function
 *     writes both into `useAuthStore` via `setSession`.
 *   - On failure (401 — no cookie, expired, or revoked): throws
 *     `ApiError`; this function treats that as "no existing session"
 *     and leaves the store cleared. This is an expected, silent case
 *     (e.g. every first-time visitor) and must never surface as an
 *     error toast to the user.
 *
 * @returns `true` if a session was restored, `false` if the visitor is
 * simply not logged in. Only throws on a genuine unexpected failure
 * (e.g. network error), which the caller may choose to log.
 */
export async function bootstrapSession(): Promise<boolean> {
  try {
    const response = await apiClient.post<RefreshTokenResponse>(
      "/auth/refresh-token",
      undefined,
      { skipInterceptors: true } // no access token exists yet to attach
    );
    useAuthStore.getState().setSession(response.user, response.accessToken);
    return true;
  } catch (error) {
    // A 401 here just means "not logged in yet" — not an error state.
    // Re-throw anything else so the caller can decide how to surface it.
    if (error instanceof ApiError && error.status === 401) {
      useAuthStore.getState().clearSession();
      return false;
    }
    throw error;
  }
}

/**
 * Placeholder — real request/response shape and error handling land with
 * the Auth UI feature. Included now so `services/auth.service.ts` is the
 * single, agreed-upon home for every auth network call.
 */
export const authService = {
  bootstrapSession,
  // login: (credentials: LoginPayload) => Promise<RefreshTokenResponse>
  // logout: () => Promise<void>
};
