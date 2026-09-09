import { beforeEach, describe, expect, it } from "vitest";
import { useAuthStore } from "@/lib/store/auth-store";

const sampleUser = {
  id: "u_1",
  fullName: "سارا احمدی",
  phone: "09120000000",
};

function resetStore() {
  useAuthStore.setState({ user: null, accessToken: null, isAuthenticated: false });
  window.localStorage.clear();
  window.sessionStorage.clear();
}

describe("auth-store — Task 1.6 storage correction", () => {
  beforeEach(resetStore);

  it("keeps accessToken in memory after setSession", () => {
    useAuthStore.getState().setSession(sampleUser, "secret-access-token");
    expect(useAuthStore.getState().accessToken).toBe("secret-access-token");
    expect(useAuthStore.getState().isAuthenticated).toBe(true);
  });

  it("never writes the access token into localStorage", () => {
    useAuthStore.getState().setSession(sampleUser, "secret-access-token");

    const rawLocalStorage = JSON.stringify(window.localStorage);
    for (let i = 0; i < window.localStorage.length; i += 1) {
      const key = window.localStorage.key(i);
      const value = key ? window.localStorage.getItem(key) : null;
      expect(value).not.toContain("secret-access-token");
    }
    expect(rawLocalStorage).not.toContain("secret-access-token");
  });

  it("never writes the access token into sessionStorage", () => {
    useAuthStore.getState().setSession(sampleUser, "secret-access-token");

    for (let i = 0; i < window.sessionStorage.length; i += 1) {
      const key = window.sessionStorage.key(i);
      const value = key ? window.sessionStorage.getItem(key) : null;
      expect(value).not.toContain("secret-access-token");
    }
  });

  it("the persisted auth-store entry never contains an accessToken field with a value", () => {
    useAuthStore.getState().setSession(sampleUser, "secret-access-token");

    const persisted = window.localStorage.getItem("auth-store");
    expect(persisted).not.toBeNull();
    const parsed = JSON.parse(persisted as string);
    // partialize only allow-lists user/isAuthenticated — accessToken
    // must be absent from the persisted state entirely.
    expect(parsed.state).not.toHaveProperty("accessToken");
    expect(parsed.state.user).toEqual(sampleUser);
    expect(parsed.state.isAuthenticated).toBe(true);
  });

  it("clearSession wipes the in-memory token", () => {
    useAuthStore.getState().setSession(sampleUser, "secret-access-token");
    useAuthStore.getState().clearSession();
    expect(useAuthStore.getState().accessToken).toBeNull();
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });
});
