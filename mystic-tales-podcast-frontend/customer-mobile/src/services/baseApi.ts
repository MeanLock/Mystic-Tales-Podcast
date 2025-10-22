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

// Simple mutex implementation
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

// Mutex to prevent multiple refresh attempts
const mutex = new SimpleMutex();

// Base query with auth handling
const baseQuery = fetchBaseQuery({
  baseUrl: "https://mystic-tale-podcast.com",
  prepareHeaders: async (headers, { getState, endpoint }) => {
    // Check for custom auth mode in meta
    const meta = (endpoint as any)?.__meta;

    // Public mode - no auth
    if (meta?.authMode === "public") {
      return headers;
    }

    // WithToken mode - use token from meta
    if (meta?.authMode === "withToken" && meta?.token) {
      headers.set("authorization", `Bearer ${meta.token}`);
      return headers;
    }

    // Auth mode (default) - use token from Redux or SecureStore
    const state = getState() as RootState;
    let token = state.auth.accessToken;

    // If no token in Redux, try to get from SecureStore
    if (!token) {
      token = await tokenStore.getAccess();
    }

    if (token) {
      headers.set("authorization", `Bearer ${token}`);
    }

    return headers;
  },
});

// Base query with auto token refresh
const baseQueryWithReauth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  // Wait if another request is refreshing token
  await mutex.waitForUnlock();

  let result = await baseQuery(args, api, extraOptions);

  // If 401 and not already refreshing, try to refresh token
  if (result.error && result.error.status === 401) {
    if (!mutex.isLocked()) {
      const release = await mutex.acquire();
      try {
        const refreshToken = await tokenStore.getRefresh();

        if (refreshToken) {
          // Try to refresh token
          const refreshResult = await baseQuery(
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

            // Save new tokens
            await tokenStore.setAccess(accessToken);
            await tokenStore.setRefresh(newRefreshToken);

            // Update Redux state
            api.dispatch(setCredentials({ user: null, accessToken }));

            // Retry original request with new token
            result = await baseQuery(args, api, extraOptions);
          } else {
            // Refresh failed - logout
            api.dispatch(logoutLocal());
            await tokenStore.clearAll();
          }
        } else {
          // No refresh token - logout
          api.dispatch(logoutLocal());
          await tokenStore.clearAll();
        }
      } finally {
        release();
      }
    } else {
      // Wait for refresh to complete and retry
      await mutex.waitForUnlock();
      result = await baseQuery(args, api, extraOptions);
    }
  }

  return result;
};

export const baseApi = createApi({
  reducerPath: "api",
  baseQuery: baseQueryWithReauth,
  tagTypes: ["FileUrl", "Episodes", "User"],
  endpoints: () => ({}),
});

// Type helpers for creating endpoints with different auth modes
export type PublicEndpoint = { authMode: "public" };
export type AuthEndpoint = { authMode: "auth" };
export type WithTokenEndpoint = { authMode: "withToken"; token: string };

// Helper to add auth mode to endpoint
export const withAuthMode = <T extends FetchArgs>(
  args: T,
  mode: PublicEndpoint | AuthEndpoint | WithTokenEndpoint
): T & { __meta: typeof mode } => {
  return { ...args, __meta: mode } as any;
};
