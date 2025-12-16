import { appApi } from "../../api/appApi";
import * as SecureStore from "expo-secure-store";
import { JwtUtil } from "../../utils/jwt";
import { setCredentials } from "@/src/features/auth/authSlice";

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

const authApi = appApi.injectEndpoints({
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
          await SecureStore.setItemAsync("accessToken", token);

          api.dispatch(setCredentials({ user: null, accessToken: token }));

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
            await SecureStore.setItemAsync(
              "device_info_token",
              device_info_token
            );
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

          api.dispatch(setCredentials({ user: account, accessToken: token }));

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
  }),
});

export const { useLoginMutation } = authApi;
