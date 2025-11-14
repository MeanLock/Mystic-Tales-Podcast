import { appApi } from "@/core/api/appApi";
import { JwtUtil } from "@/core/utils/token";
import { setAuthToken, setUser } from "@/redux/slices/authSlice/authSlice";

interface LoginResponse {
  isError: boolean;
  message: string;
  isUnVerified?: boolean;
}

interface GetMeResponse {
  Account: {
    Id: number;
    Email: string;
    FullName: string;
    Dob: string;
    Gender: string;
    Address: string;
    Phone: string;
    Balance: number;
    MainImageFileKey: string;
    PodcastListenSlot: number;
    DeactivatedAt: string;
    IsPodcaster: boolean;
  };
}

export const authApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    login: build.mutation<
      LoginResponse,
      {
        ManualLoginInfo: { Email: string; Password: string };
        DeviceInfo: { DeviceId: string; Platform: string; OSName: string };
      }
    >({
      async queryFn(
        { ManualLoginInfo, DeviceInfo },
        api,
        _extraOptions,
        baseQuery
      ) {
        let loginResponse: LoginResponse = {
          isError: false,
          message: "",
          isUnVerified: false,
        };

        try {
          // 1️⃣ Gọi saga login
          const sagaRes = await api
            .dispatch(
              appApi.endpoints.kickoffThenWait.initiate({
                kickoff: {
                  url: "/api/user-service/api/auth/login-manual",
                  method: "POST",
                  body: { ManualLoginInfo, DeviceInfo },
                  authMode: "public",
                },
                poll: { intervalMs: 1000, maxAttempts: 30 },
              })
            )
            .unwrap()
            .catch((err) => {
              console.error("Login saga failed:", err);
              return null;
            });

          if (!sagaRes) {
            return { data: { isError: true, message: "Login saga failed" } };
          }
          console.log("Sage Response: ", sagaRes);
          if (!sagaRes.AccessToken) {
            return {
              data: { isError: true, message: "No access token returned" },
            };
          }

          const token = sagaRes.AccessToken;
          localStorage.setItem("accessToken", token);
          api.dispatch(setAuthToken(token));

          // 3️⃣ Kiểm tra role
          const { role_id, device_info_token } = JwtUtil.decodeToken(token);
          if (role_id !== "1") {
            return {
              data: {
                isError: true,
                message: "You're not the Customer! Please use another web",
              },
            };
          }
          if (device_info_token) {
            localStorage.setItem("device_info_token", device_info_token);
          } else {
            return {
              data: {
                isError: true,
                message: "Cannot specify device, please login again",
              },
            };
          }

          // 4️⃣ Gọi get-me
          const accountMeRes = await baseQuery({
            url: "/api/user-service/api/accounts/me",
            method: "GET",
            authMode: "required",
          });

          if (!accountMeRes.data) {
            return {
              data: { isError: true, message: "Failed to get account info" },
            };
          }

          const accountData = accountMeRes.data as GetMeResponse;
          const account = accountData.Account;
          if (account.DeactivatedAt) {
            return {
              data: {
                isError: true,
                message: "Account Is Deactivated!",
                isUnVerified: false,
              },
            };
          }
          // 5️⃣ Lấy ImageUrl
          let ImageUrl = "/images/unknown/user.jpg";
          if (account.MainImageFileKey) {
            try {
              const imageRes = await baseQuery({
                url: `/api/user-service/api/misc/public-source/get-file-url/${account.MainImageFileKey}`,
                method: "GET",
                authMode: "public",
              });
              ImageUrl = (imageRes.data as any)?.FileUrl ?? ImageUrl;
            } catch {
              console.warn(
                "Failed to fetch account image, fallback to default"
              );
            }
          }

          const accountWithImage = { ...account, ImageUrl };
          delete (accountWithImage as any).MainImageFileKey;

          api.dispatch(setUser(accountWithImage));

          loginResponse = { isError: false, message: "Login successful" };
        } catch (error) {
          console.error("Login error:", error);
          loginResponse = {
            isError: true,
            message: "Something went wrong, please try again later",
          };
        }

        return { data: loginResponse };
      },
    }),

    loginGoogle: build.mutation<
      LoginResponse,
      {
        GoogleAuth: { AuthorizationCode: string; RedirectUri: string };
        DeviceInfo: { DeviceId: string; Platform: string; OSName: string };
      }
    >({
      async queryFn({ GoogleAuth, DeviceInfo }, api, _extraOptions, baseQuery) {
        let loginResponse: LoginResponse = {
          isError: false,
          message: "",
          isUnVerified: false,
        };

        try {
          // 1️⃣ Gọi saga login
          const sagaRes = await api
            .dispatch(
              appApi.endpoints.kickoffThenWait.initiate({
                kickoff: {
                  url: "/api/user-service/api/auth/login-google",
                  method: "POST",
                  body: { GoogleAuth, DeviceInfo },
                  authMode: "public",
                },
                poll: { intervalMs: 1000, maxAttempts: 30 },
              })
            )
            .unwrap()
            .catch((err) => {
              console.error("Login Google saga failed:", err);
              return null;
            });

          if (!sagaRes) {
            return {
              data: { isError: true, message: "Login Google saga failed" },
            };
          }
          console.log("Google Sage Response: ", sagaRes);
          if (!sagaRes.AccessToken) {
            return {
              data: { isError: true, message: "No access token returned" },
            };
          }

          const token = sagaRes.AccessToken;
          localStorage.setItem("accessToken", token);
          api.dispatch(setAuthToken(token));

          // 3️⃣ Kiểm tra role
          const { role_id, device_info_token } = JwtUtil.decodeToken(token);
          if (role_id !== "1") {
            return {
              data: {
                isError: true,
                message: "You're not the Customer! Please use another web",
              },
            };
          }
          if (device_info_token) {
            localStorage.setItem("device_info_token", device_info_token);
          } else {
            return {
              data: {
                isError: true,
                message: "Cannot specify device, please login again",
              },
            };
          }

          // 4️⃣ Gọi get-me
          const accountMeRes = await baseQuery({
            url: "/api/user-service/api/accounts/me",
            method: "GET",
            authMode: "required",
          });

          if (!accountMeRes.data) {
            return {
              data: { isError: true, message: "Failed to get account info" },
            };
          }

          const accountData = accountMeRes.data as GetMeResponse;
          const account = accountData.Account;
          if (account.DeactivatedAt) {
            return {
              data: {
                isError: true,
                message: "Account Is Deactivated!",
                isUnVerified: false,
              },
            };
          }
          // 5️⃣ Lấy ImageUrl
          let ImageUrl = "/images/unknown/user.jpg";
          if (account.MainImageFileKey) {
            try {
              const imageRes = await baseQuery({
                url: `/api/user-service/api/misc/public-source/get-file-url/${account.MainImageFileKey}`,
                method: "GET",
                authMode: "public",
              });
              ImageUrl = (imageRes.data as any)?.FileUrl ?? ImageUrl;
            } catch {
              console.warn(
                "Failed to fetch account image, fallback to default"
              );
            }
          }

          const accountWithImage = { ...account, ImageUrl };
          delete (accountWithImage as any).MainImageFileKey;

          api.dispatch(setUser(accountWithImage));

          loginResponse = { isError: false, message: "Login successful" };
        } catch (error) {
          console.error("Login Google error:", error);
          loginResponse = {
            isError: true,
            message: "Something went wrong, please try again later",
          };
        }

        return { data: loginResponse };
      },
    }),

    sendForgotPasswordRequest: build.mutation<
      { Message: string },
      { Email: string }
    >({
      async queryFn({ Email }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/user-service/api/auth/forgot-password",
                method: "POST",
                body: { Email },
                authMode: "public",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),

    resetForgotPassword: build.mutation<
      { Message: string },
      { Email: string; ResetPasswordToken: string; NewPassword: string }
    >({
      async queryFn({ Email, ResetPasswordToken, NewPassword }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/user-service/api/auth/new-reset-password",
                method: "POST",
                body: {
                  ResetPasswordInfo: { Email, ResetPasswordToken, NewPassword },
                },
                authMode: "public",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),

    register: build.mutation<{ Message: string }, { formData: any }>({
      async queryFn({ formData }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/user-service/api/auth/register/customer",
                method: "POST",
                body: formData,
                authMode: "public",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),

    verifyAccount: build.mutation<
      { Message: string },
      { Email: string; VerifyCode: string }
    >({
      async queryFn({ Email, VerifyCode }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/user-service/api/auth/account-verification",
                method: "POST",
                body: {
                  AccountVerificationInfo: {
                    Email: Email,
                    VerifyCode: VerifyCode,
                  },
                },
                authMode: "public",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),

    updatePassword: build.mutation<
      { Message: string },
      { CurrentPassword: string; NewPassword: string }
    >({
      async queryFn({ CurrentPassword, NewPassword }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: "/api/user-service/api/auth/update-password",
                method: "POST",
                body: {
                  PasswordUpdateInfo: {
                    OldPassword: CurrentPassword,
                    NewPassword: NewPassword,
                  },
                },
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),
  }),
});

export const {
  useLoginMutation,
  useLoginGoogleMutation,
  useSendForgotPasswordRequestMutation,
  useResetForgotPasswordMutation,
  useRegisterMutation,
  useVerifyAccountMutation,
  useUpdatePasswordMutation,
} = authApi;
