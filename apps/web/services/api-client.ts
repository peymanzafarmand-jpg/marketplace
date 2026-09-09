import { ApiError } from "@/types/api";

/**
 * Thin fetch wrapper shared by every service module in `services/`.
 * Business logic (endpoints, DTOs) belongs in per-feature service files
 * (e.g. `services/products.service.ts`), not here. This file only knows
 * how to talk to the backend: base URL, headers, auth token, retries,
 * and turning failed responses into a single `ApiError` type.
 */

// Falls back to the local Docker Compose API (docs/repo-contract.md: internal port
// 8080, versioned route prefix api/v1) when NEXT_PUBLIC_API_BASE_URL isn't set.
const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080/api/v1";

type RequestInterceptor = (init: RequestInit) => RequestInit | Promise<RequestInit>;
type ResponseInterceptor = (response: Response) => Response | Promise<Response>;

const requestInterceptors: RequestInterceptor[] = [];
const responseInterceptors: ResponseInterceptor[] = [];

/** Register a function that can mutate every outgoing request (e.g. attach auth header). */
export function addRequestInterceptor(fn: RequestInterceptor) {
  requestInterceptors.push(fn);
}

/** Register a function that inspects/transforms every raw response before parsing. */
export function addResponseInterceptor(fn: ResponseInterceptor) {
  responseInterceptors.push(fn);
}

// --- Auth-ready default interceptor -----------------------------------
// Reads the access token from the auth store at request time. The store
// itself decides how the token is persisted (see lib/store/auth-store.ts).
// Kept as the first interceptor so features can add more after it.
addRequestInterceptor(async (init) => {
  const { useAuthStore } = await import("@/lib/store/auth-store");
  const token = useAuthStore.getState().accessToken;
  if (!token) return init;
  return {
    ...init,
    headers: {
      ...init.headers,
      Authorization: `Bearer ${token}`,
    },
  };
});

interface RequestOptions extends Omit<RequestInit, "body"> {
  body?: unknown;
  /** Skip the auth/other interceptors for this one call (e.g. login, refresh). */
  skipInterceptors?: boolean;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, skipInterceptors, headers, ...rest } = options;

  let init: RequestInit = {
    ...rest,
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  };

  if (!skipInterceptors) {
    for (const interceptor of requestInterceptors) {
      init = await interceptor(init);
    }
  }

  let response: Response;
  try {
    response = await fetch(`${BASE_URL}${path}`, init);
  } catch {
    // Network failure (offline, DNS, CORS, timeout at the transport level)
    throw new ApiError({
      code: "NETWORK_ERROR",
      message: "ارتباط با سرور برقرار نشد. اتصال اینترنت خود را بررسی کنید.",
      status: 0,
    });
  }

  for (const interceptor of responseInterceptors) {
    response = await interceptor(response);
  }

  if (!response.ok) {
    const payload = await safeJson(response);
    throw new ApiError({
      code: payload?.error?.code ?? `HTTP_${response.status}`,
      message: payload?.error?.message ?? defaultMessageFor(response.status),
      status: response.status,
      fields: payload?.error?.fields,
    });
  }

  if (response.status === 204) return undefined as T;
  return (await safeJson(response)) as T;
}

async function safeJson(response: Response) {
  try {
    return await response.json();
  } catch {
    return null;
  }
}

function defaultMessageFor(status: number): string {
  if (status === 401) return "برای ادامه باید وارد حساب کاربری خود شوید.";
  if (status === 403) return "دسترسی به این بخش برای شما مجاز نیست.";
  if (status === 404) return "موردی که دنبالش بودید پیدا نشد.";
  if (status >= 500) return "خطایی در سرور رخ داده. لطفاً بعداً تلاش کنید.";
  return "درخواست با خطا مواجه شد.";
}

export const apiClient = {
  get: <T>(path: string, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "GET" }),
  post: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "POST", body }),
  put: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "PUT", body }),
  patch: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "PATCH", body }),
  delete: <T>(path: string, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "DELETE" }),
};
