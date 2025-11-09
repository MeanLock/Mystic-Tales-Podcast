import { baseApi, withAuthMode } from "../baseApi";
import { tokenStore } from "@/src/features/auth/tokenStore";
import { setCredentials } from "@/src/features/auth/authSlice";
import { pollSagaResult } from "../sagaPolling";
import type { User, UserFromAPI, UserUI } from "@/src/types/user";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { JwtUtil } from "@/src/utils/token";
import { fetchPublicFileUrl } from "../file/publicSource/getPublicSource.service";

// Types
type LoginRequest = {
  ManualLoginInfo: {
    Email: string;
    Password: string;
  };
};

type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  // user may be null if not available
  user: User | null;
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

          console.log("[authApi] sagaResult.data:", sagaResult.data);
          console.log(
            "[authApi] AccessToken type/len:",
            typeof AccessToken,
            AccessToken ? AccessToken.length : "<empty>"
          );
          console.log(
            "[authApi] RefreshToken type/len:",
            typeof RefreshToken,
            RefreshToken ? "len=" + RefreshToken.length : "<missing>"
          );

          // Validate AccessToken exists (required). RefreshToken is optional.
          if (typeof AccessToken !== "string") {
            return {
              error: {
                status: "CUSTOM_ERROR",
                error: "Invalid or missing AccessToken returned from saga",
              } as FetchBaseQueryError,
            };
          }

          // Step 3: Save tokens to SecureStore
          await tokenStore.setAccess(AccessToken);
          if (typeof RefreshToken === "string") {
            await tokenStore.setRefresh(RefreshToken);
          }

          const userDecoded = JwtUtil.decodeToken(AccessToken);

          // Step 4: Get user info with the new token — API now returns Account directly
          const meArgs = withAuthMode(
            {
              url: "/api/user-service/api/accounts/me",
              method: "GET",
            },
            { authMode: "withToken", token: AccessToken }
          );

          const meResult = await baseQuery(meArgs);

          if ("error" in meResult) {
            return { error: meResult.error as FetchBaseQueryError };
          }

          // Expect payload like: { Account: { ... } }
          const accountPayload = meResult.data as
            | { Account?: UserFromAPI }
            | any;
          const userFromApi = accountPayload?.Account;

          // 5️⃣ Resolve avatar using fetchPublicFileUrl()
          let finalUser: UserUI | null = null;

          if (userFromApi) {
            const { MainImageFileKey, ...rest } = userFromApi; // omit MainImageFileKey
            console.log(
              "[authApi] userFromApi.MainImageFileKey:",
              MainImageFileKey
            );

            // If we have a key, try to resolve it. Log before/after for diagnostics.
            let imageUrl = "/assets/images/user/unknown.jpg";
            if (MainImageFileKey) {
              console.log(
                "[authApi] resolving avatar for key -> calling fetchPublicFileUrl"
              );
              imageUrl = await fetchPublicFileUrl(
                MainImageFileKey,
                api,
                extraOptions
              );
              console.log("[authApi] fetchPublicFileUrl returned:", imageUrl);
            } else {
              console.log(
                "[authApi] MainImageFileKey is falsy — using placeholder"
              );
              imageUrl =
                "https://i.pinimg.com/736x/62/07/15/620715d7b709a2f7f137227885c66793.jpg";
            }

            finalUser = {
              ...(rest as any),
              ImageUrl: imageUrl, // replace MainImageFileKey with ImageUrl
            } as UserUI;
          } else if (userDecoded) {
            const d = userDecoded as any;
            finalUser = {
              Id: d?.Id ?? d?.id ?? 0,
              Email: d?.Email ?? "",
              FullName: d?.FullName ?? "",
              Dob: d?.Dob ?? "",
              Gender: d?.Gender ?? "",
              Address: d?.Address ?? "",
              Phone: d?.Phone ?? "",
              Balance: d?.Balance ?? 0,
              ImageUrl: "", // no MainImageFileKey, just ImageUrl
              PodcastListenSlot: d?.PodcastListenSlot ?? 0,
              DeactivatedAt: d?.DeactivatedAt ?? "",
              IsPodcaster: d?.IsPodcaster ?? false,
            };
          }

          // Step 6: Update Redux state
          api.dispatch(
            setCredentials({ user: finalUser, accessToken: AccessToken })
          );

          // Step 7: Return combined data
          return {
            data: {
              accessToken: AccessToken,
              refreshToken: RefreshToken,
              user: finalUser,
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
