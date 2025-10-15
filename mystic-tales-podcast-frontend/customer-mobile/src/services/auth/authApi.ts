import { baseApi, withAuthMode } from "../baseApi";
import { tokenStore } from "@/src/features/auth/tokenStore";
import { setCredentials } from "@/src/features/auth/authSlice";
import { pollSagaResult } from "../sagaPolling";
import type { User } from "@/src/types/user";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";

// Types
type LoginRequest = {
  LoginInfo: {
    Email: string;
    Password: string;
  };
};

type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  user: User;
};

type SagaResponse = {
  SagaInstanceId: string;
};

// Auth API
export const authApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    // Login with Saga polling
    login: build.mutation<LoginResponse, LoginRequest>({
      queryFn: async (body, api, extraOptions, baseQuery) => {
        try {
          // Step 1: Call login API (public)
          const loginArgs = withAuthMode(
            {
              url: "/api/user-service/api/auth/login-manual",
              method: "POST",
              body,
            },
            { authMode: "public" }
          );

          const loginResult = await baseQuery(loginArgs);

          if ("error" in loginResult) {
            return { error: loginResult.error as FetchBaseQueryError };
          }

          const { SagaInstanceId } = loginResult.data as SagaResponse;

          if (!SagaInstanceId) {
            return {
              error: {
                status: "CUSTOM_ERROR",
                error: "No SagaInstanceId returned",
              } as FetchBaseQueryError,
            };
          }

          // Step 2: Poll saga orchestrator
          const sagaResult = await pollSagaResult<{
            AccessToken: string;
            RefreshToken: string;
          }>({
            sagaId: SagaInstanceId,
            baseQuery,
            api,
            extraOptions,
            timeoutSeconds: 120,
            intervalSeconds: 2,
          });

          if (sagaResult.status !== "SUCCESS" || !sagaResult.data) {
            return {
              error: {
                status: "CUSTOM_ERROR",
                error: sagaResult.error || "Saga polling failed",
              } as FetchBaseQueryError,
            };
          }

          const { AccessToken, RefreshToken } = sagaResult.data;

          // Step 3: Save tokens to SecureStore
          await tokenStore.setAccess(AccessToken);
          await tokenStore.setRefresh(RefreshToken);

          // Step 4: Get user info with the new token
          const meArgs = withAuthMode(
            {
              url: "/api/user-service/api/auth/me",
              method: "GET",
            },
            { authMode: "withToken", token: AccessToken }
          );

          const meResult = await baseQuery(meArgs);

          if ("error" in meResult) {
            return { error: meResult.error as FetchBaseQueryError };
          }

          const { SagaInstanceId: meSagaId } = meResult.data as SagaResponse;

          if (!meSagaId) {
            return {
              error: {
                status: "CUSTOM_ERROR",
                error: "No SagaInstanceId returned from /me endpoint",
              } as FetchBaseQueryError,
            };
          }

          // Step 5: Poll saga orchestrator for user info
          const userSagaResult = await pollSagaResult<User>({
            sagaId: meSagaId,
            baseQuery,
            api,
            extraOptions,
            timeoutSeconds: 120,
            intervalSeconds: 2,
          });

          if (userSagaResult.status !== "SUCCESS" || !userSagaResult.data) {
            return {
              error: {
                status: "CUSTOM_ERROR",
                error: userSagaResult.error || "Failed to fetch user info",
              } as FetchBaseQueryError,
            };
          }

          const user = userSagaResult.data;

          // Step 6: Update Redux state
          api.dispatch(setCredentials({ user, accessToken: AccessToken }));

          // Step 7: Return combined data
          return {
            data: {
              accessToken: AccessToken,
              refreshToken: RefreshToken,
              user,
            },
          };
        } catch (error) {
          return {
            error: {
              status: "FETCH_ERROR",
              error: String(error),
            } as FetchBaseQueryError,
          };
        }
      },
    }),

    // Get current user
    // getMe: build.query<User, void>({
    //   query: () => ({
    //     url: "/api/user-service/api/auth/me",
    //     method: "GET",
    //   }),
    //   providesTags: ["User"],
    // }),

    // Logout
    logout: build.mutation<void, void>({
      queryFn: async (_, api) => {
        // Clear tokens from SecureStore
        await tokenStore.clearAll();

        // Clear Redux state
        api.dispatch(setCredentials({ user: null, accessToken: null }));

        return { data: undefined };
      },
    }),

    // Refresh token (called automatically by baseQuery on 401)
    refreshToken: build.mutation<
      { accessToken: string; refreshToken: string },
      { refreshToken: string }
    >({
      query: (body) =>
        withAuthMode(
          {
            url: "/api/user-service/api/auth/refresh",
            method: "POST",
            body,
          },
          { authMode: "public" }
        ),
    }),
  }),
  overrideExisting: false,
});

export const {
  useLoginMutation,
  // useGetMeQuery,
  // useLazyGetMeQuery,
  useLogoutMutation,
  useRefreshTokenMutation,
} = authApi;
