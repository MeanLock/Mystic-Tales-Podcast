import {
  createApi,
  fetchBaseQuery,
  type BaseQueryFn,
  type FetchArgs,
  type FetchBaseQueryError,
} from "@reduxjs/toolkit/query/react";
import type { RootState } from "@/src/store/store";
import { tokenStore } from "@/src/features/auth/tokenStore";
import { setCredentials, logoutLocal } from "@/src/features/auth/authSlice";

/* --------------------------- 🔒 Simple Mutex --------------------------- */
class SimpleMutex {
  private locked = false;
  private queue: (() => void)[] = [];

  async acquire(): Promise<() => void> {
    while (this.locked) {
      await new Promise<void>((resolve) => this.queue.push(resolve));
    }
    this.locked = true;
    return () => {
      this.locked = false;
      const resolve = this.queue.shift();
      if (resolve) resolve();
    };
  }

  isLocked(): boolean {
    return this.locked;
  }

  async waitForUnlock(): Promise<void> {
    while (this.locked) {
      await new Promise<void>((resolve) => this.queue.push(resolve));
    }
  }
}

const mutex = new SimpleMutex();

/* --------------------------- ⚙️ Base Query Core --------------------------- */
const rawBaseQuery = fetchBaseQuery({
  baseUrl: "https://3e8548f2269f.ngrok-free.app",
  prepareHeaders: async (headers, { getState, endpoint }) => {
    const meta = (endpoint as any)?.__meta;

    // 1️⃣ Public mode
    if (meta?.authMode === "public") return headers;

    // 2️⃣ WithToken mode
    if (meta?.authMode === "withToken" && meta?.token) {
      headers.set("authorization", `Bearer ${meta.token}`);
      return headers;
    }

    // 3️⃣ Auth mode (default)
    const state = getState() as RootState;
    let token = state.auth.accessToken;
    if (!token) token = await tokenStore.getAccess();

    if (token) {
      headers.set("authorization", `Bearer ${token}`);
      headers.set("ngrok-skip-browser-warning", "69420");
    }

    return headers;
  },
});

/* --------------------------- 🧩 Base Query With Logging + Reauth --------------------------- */
const baseQueryWithReauth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  // Wait if another request is refreshing
  await mutex.waitForUnlock();

  // For logging
  const method = typeof args === "string" ? "GET" : args.method ?? "GET";
  const url =
    typeof args === "string"
      ? args
      : `${args.url?.startsWith("http") ? args.url : args.url ?? ""}`;
  const body = typeof args === "string" ? undefined : args.body;

  console.log("➡️ [RTKQ] Request:", {
    url,
    method,
    body,
  });

  const start = Date.now();
  let result = await rawBaseQuery(args, api, extraOptions);
  const timeMs = Date.now() - start;

  const status =
    (result as any).meta?.response?.status ??
    (result as any).error?.status ??
    "unknown";

  console.log("⬅️ [RTKQ] Response:", {
    url,
    status,
    timeMs,
    payload: result.data ?? result.error,
  });

  // 401 handling
  if (result.error && result.error.status === 401) {
    if (!mutex.isLocked()) {
      const release = await mutex.acquire();
      try {
        const refreshToken = await tokenStore.getRefresh();

        if (refreshToken) {
          console.log("🔄 [RTKQ] Refreshing token...");
          const refreshResult = await rawBaseQuery(
            {
              url: "/api/user-service/api/auth/refresh",
              method: "POST",
              body: { refreshToken },
            },
            api,
            extraOptions
          );

          if (refreshResult.data) {
            const { accessToken, refreshToken: newRefreshToken } =
              refreshResult.data as {
                accessToken: string;
                refreshToken: string;
              };

            await tokenStore.setAccess(accessToken);
            await tokenStore.setRefresh(newRefreshToken);

            api.dispatch(setCredentials({ user: null, accessToken }));

            // Retry original request
            result = await rawBaseQuery(args, api, extraOptions);

            console.log("✅ [RTKQ] Retried after refresh:", {
              url,
              status:
                (result as any).meta?.response?.status ??
                (result as any).error?.status ??
                "unknown",
            });
          } else {
            console.log("❌ [RTKQ] Token refresh failed – logging out");
            api.dispatch(logoutLocal());
            await tokenStore.clearAll();
          }
        } else {
          console.log("⚠️ [RTKQ] No refresh token – logging out");
          api.dispatch(logoutLocal());
          await tokenStore.clearAll();
        }
      } finally {
        release();
      }
    } else {
      console.log("⏳ [RTKQ] Waiting for token refresh to complete...");
      await mutex.waitForUnlock();
      result = await rawBaseQuery(args, api, extraOptions);
    }
  }

  return result;
};

/* --------------------------- 🌐 RTK API --------------------------- */
export const baseApi = createApi({
  reducerPath: "api",
  baseQuery: baseQueryWithReauth,
  tagTypes: ["FileUrl", "Episodes", "User"],
  endpoints: () => ({}),
});

/* --------------------------- 🧠 Helpers --------------------------- */
export type PublicEndpoint = { authMode: "public" };
export type AuthEndpoint = { authMode: "auth" };
export type WithTokenEndpoint = { authMode: "withToken"; token: string };
export { rawBaseQuery as baseQuery }; // 👈 Export baseQuery để dùng ở ngoài

export const withAuthMode = <T extends FetchArgs>(
  args: T,
  mode: PublicEndpoint | AuthEndpoint | WithTokenEndpoint
): T & { __meta: typeof mode } => {
  return { ...args, __meta: mode } as any;
};
