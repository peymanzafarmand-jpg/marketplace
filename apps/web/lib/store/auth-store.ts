import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { User } from "@/types/domain";

interface AuthState {
  user: User | null;
  /**
   * In-memory only. NEVER persisted — see `partialize` below. Read fresh
   * on every request by the interceptor in `services/api-client.ts`; the
   * only durable source of truth for a session surviving a page reload
   * is the HttpOnly refresh-token cookie the backend sets, recovered via
   * `bootstrapSession()` in `services/auth.service.ts`.
   */
  accessToken: string | null;
  isAuthenticated: boolean;
  /** Sets the session after a successful login/refresh call. */
  setSession: (user: User, accessToken: string) => void;
  /** Clears the session on logout or a 401 from the API. */
  clearSession: () => void;
}

/**
 * Foundation only: holds the session shape and basic setters. Actual
 * login/refresh/logout network calls belong in `services/auth.service.ts`
 * + a future `features/auth` hook, which will call these setters after
 * the request resolves.
 *
 * SECURITY (Task 1.6 correction): `persist` still backs `user` /
 * `isAuthenticated` with localStorage — those are non-sensitive UI state
 * (e.g. "show the account menu as logged in" before bootstrap resolves)
 * and are safe to persist. `accessToken` is deliberately excluded via
 * `partialize` so it is never written to localStorage, sessionStorage,
 * or any JS-readable cookie. It lives in memory only and is lost on
 * every full page reload — that is intentional. The token is recovered
 * by calling `bootstrapSession()` on app start, which exchanges the
 * HttpOnly refresh-token cookie (invisible to JavaScript, so not
 * exfiltratable via XSS) for a fresh access token.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      isAuthenticated: false,
      setSession: (user, accessToken) =>
        set({ user, accessToken, isAuthenticated: true }),
      clearSession: () => set({ user: null, accessToken: null, isAuthenticated: false }),
    }),
    {
      name: "auth-store",
      // Explicit allow-list: only these keys are ever written to
      // localStorage. accessToken must never appear here.
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
