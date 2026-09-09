import { beforeEach, describe, expect, it, vi } from "vitest";
import { useAuthStore } from "@/lib/store/auth-store";
import { ApiError } from "@/types/api";

const { postMock } = vi.hoisted(() => ({ postMock: vi.fn() }));

vi.mock("@/services/api-client", () => ({
  apiClient: { post: postMock },
}));

describe("bootstrapSession", () => {
  beforeEach(() => {
    postMock.mockReset();
    useAuthStore.setState({ user: null, accessToken: null, isAuthenticated: false });
  });

  it("restores the session in memory when the refresh call succeeds", async () => {
    const { bootstrapSession } = await import("@/services/auth.service");
    const user = { id: "u_1", fullName: "سارا احمدی", phone: "09120000000" };
    postMock.mockResolvedValueOnce({ user, accessToken: "fresh-token" });

    const result = await bootstrapSession();

    expect(result).toBe(true);
    expect(postMock).toHaveBeenCalledWith(
      "/auth/refresh-token",
      undefined,
      expect.objectContaining({ skipInterceptors: true })
    );
    expect(useAuthStore.getState().accessToken).toBe("fresh-token");
    expect(useAuthStore.getState().isAuthenticated).toBe(true);
  });

  it("silently clears the session on a 401 (visitor was never logged in)", async () => {
    const { bootstrapSession } = await import("@/services/auth.service");
    postMock.mockRejectedValueOnce(
      new ApiError({ code: "UNAUTHORIZED", message: "no session", status: 401 })
    );

    const result = await bootstrapSession();

    expect(result).toBe(false);
    expect(useAuthStore.getState().accessToken).toBeNull();
    expect(useAuthStore.getState().isAuthenticated).toBe(false);
  });

  it("re-throws unexpected errors instead of swallowing them", async () => {
    const { bootstrapSession } = await import("@/services/auth.service");
    postMock.mockRejectedValueOnce(
      new ApiError({ code: "SERVER_ERROR", message: "boom", status: 500 })
    );

    await expect(bootstrapSession()).rejects.toThrow("boom");
  });
});
