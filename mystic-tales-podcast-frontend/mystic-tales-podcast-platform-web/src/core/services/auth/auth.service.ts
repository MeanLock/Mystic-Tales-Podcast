import { appApi } from "@/core/api/appApi";
import { alertMessages } from "@/core/data/alert-message.data";
import type { AlertMessage } from "@/core/types/alert";
import { JwtUtil } from "@/core/utils/token";
import { setAuthToken, setUser } from "@/redux/slices/authSlice/authSlice";

interface LoginResponse {
  isError: boolean;
  message: AlertMessage;
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
          message: {
            id: "",
            description: "",
            title: "",
            type: "info",
          },
        };

        try {
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
            .unwrap();
          // SAGA KHÔNG CÓ ACCESS TOKEN BÊN TRONG
          if (!sagaRes.AccessToken) {
            return {
              data: {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-1") ||
                  alertMessages[0],
              },
            };
          }

          const token = sagaRes.AccessToken;
          localStorage.setItem("accessToken", token);
          api.dispatch(setAuthToken(token));

          // 3️⃣ Kiểm tra role
          const { role_id, device_info_token } = JwtUtil.decodeToken(token);

          // ERROR MESSAGE: ROLE KHÔNG PHẢI CUSTOMER
          if (role_id !== "1") {
            return {
              data: {
                isError: true,
                message: alertMessages.find(
                  (msg) => msg.id === "login-failed-5"
                )!,
              },
            };
          }
          if (device_info_token) {
            localStorage.setItem("device_info_token", device_info_token);
          } else {
            // ERROR MESSAGE: KHÔNG BẮT ĐƯỢC DEVICE TOKEN
            return {
              data: {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-1") ||
                  alertMessages[0],
              },
            };
          }

          // 4️⃣ Gọi get-me
          const accountMeRes = await baseQuery({
            url: "/api/user-service/api/accounts/me",
            method: "GET",
            authMode: "required",
          });

          // ERROR MESSAGE: KHÔNG LẤY ĐƯỢC ACCOUNT ME
          if (!accountMeRes.data) {
            return {
              data: {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-1") ||
                  alertMessages[0],
              },
            };
          }

          const accountData = accountMeRes.data as GetMeResponse;
          const account = accountData.Account;

          // ERROR MESSAGE: ACCOUNT BỊ DEACTIVATED
          if (account.DeactivatedAt) {
            return {
              data: {
                isError: true,
                message: alertMessages.find(
                  (msg) => msg.id === "login-failed-3"
                )!,
              },
            };
          }

          api.dispatch(setUser(accountData.Account));

          loginResponse = {
            isError: false,
            message:
              alertMessages.find((msg) => msg.id === "login-success") ||
              alertMessages[0],
          };
        } catch (error: any) {
          // Kiểm tra loại lỗi từ polling.ts
          if (error?.kind === "SAGA_FAILED") {
            console.log("🔴 SAGA_FAILED detected");
            // ERROR MESSAGE: SAI MẬT KHẨU HOẶC EMAIL
            if (error.message?.includes("Email or password is incorrect")) {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-2") ||
                  alertMessages[0],
              };
              // ERROR MESSAGE: BỊ DEACTIVATED
            } else if (error.message.includes("Account has been deactivated")) {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-3") ||
                  alertMessages[0],
              };
              // ERROR MESSAGE: CHƯA VERIFIED
            } else if (
              error.message.includes("Account has not been verified")
            ) {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-4") ||
                  alertMessages[0],
              };
              // ERROR MESSAGE: KHÁC
            } else {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-1") ||
                  alertMessages[0],
              };
            }
          } else {
            console.log("🌐 Network or other error detected");
            // ERROR MESSAGE: LỖI MẠNG HOẶC LỖI KHÔNG XÁC ĐỊNH
            loginResponse = {
              isError: true,
              message:
                alertMessages.find((msg) => msg.id === "login-failed-1") ||
                alertMessages[0],
            };
          }
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
          message: {
            id: "",
            description: "",
            title: "",
            type: "info",
          },
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
            .unwrap();

          // SAGA KHÔNG CÓ ACCESS TOKEN BÊN TRONG
          if (!sagaRes.AccessToken) {
            return {
              data: {
                isError: true,
                message: alertMessages.find(
                  (msg) => msg.id === "login-failed-1"
                )!,
              },
            };
          }

          const token = sagaRes.AccessToken;
          localStorage.setItem("accessToken", token);
          api.dispatch(setAuthToken(token));

          // 3️⃣ Kiểm tra role
          const { role_id, device_info_token } = JwtUtil.decodeToken(token);
          // ERROR MESSAGE: ROLE KHÔNG PHẢI CUSTOMER
          if (role_id !== "1") {
            return {
              data: {
                isError: true,
                message: alertMessages.find(
                  (msg) => msg.id === "login-failed-5"
                )!,
              },
            };
          }
          if (device_info_token) {
            localStorage.setItem("device_info_token", device_info_token);
          } else {
            // ERROR MESSAGE: KHÔNG BẮT ĐƯỢC DEVICE TOKEN
            return {
              data: {
                isError: true,
                message: alertMessages.find(
                  (msg) => msg.id === "login-failed-1"
                )!,
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
              data: {
                isError: true,
                message: alertMessages.find(
                  (msg) => msg.id === "login-failed-1"
                )!,
              },
            };
          }

          const accountData = accountMeRes.data as GetMeResponse;
          const account = accountData.Account;

          // ERROR MESSAGE: ACCOUNT BỊ DEACTIVATED
          if (account.DeactivatedAt) {
            return {
              data: {
                isError: true,
                message: alertMessages.find(
                  (msg) => msg.id === "login-failed-3"
                )!,
              },
            };
          }

          api.dispatch(setUser(accountData.Account));

          loginResponse = {
            isError: false,
            message:
              alertMessages.find((msg) => msg.id === "login-success") ||
              alertMessages[0],
          };
        } catch (error: any) {
          // Kiểm tra loại lỗi từ polling.ts
          if (error?.kind === "SAGA_FAILED") {
            console.log("🔴 SAGA_FAILED detected");
            // ERROR MESSAGE: SAI MẬT KHẨU HOẶC EMAIL
            if (error.message?.includes("Email or password is incorrect")) {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-2") ||
                  alertMessages[0],
              };
              // ERROR MESSAGE: BỊ DEACTIVATED
            } else if (error.message.includes("Account has been deactivated")) {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-3") ||
                  alertMessages[0],
              };
              // ERROR MESSAGE: CHƯA VERIFIED
            } else if (
              error.message.includes("Account has not been verified")
            ) {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-4") ||
                  alertMessages[0],
              };
              // ERROR MESSAGE: KHÁC
            } else {
              loginResponse = {
                isError: true,
                message:
                  alertMessages.find((msg) => msg.id === "login-failed-1") ||
                  alertMessages[0],
              };
            }
          } else {
            console.log("🌐 Network or other error detected");
            // ERROR MESSAGE: LỖI MẠNG HOẶC LỖI KHÔNG XÁC ĐỊNH
            loginResponse = {
              isError: true,
              message:
                alertMessages.find((msg) => msg.id === "login-failed-1") ||
                alertMessages[0],
            };
          }
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
