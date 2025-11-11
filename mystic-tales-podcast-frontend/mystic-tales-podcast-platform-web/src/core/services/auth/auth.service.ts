import { appApi } from "@/core/api/appApi";
import { parseResultData } from "@/core/utils/parseResultData";
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
      { ManualLoginInfo: { Email: string; Password: string } }
    >({
      async queryFn({ ManualLoginInfo }, api, extraOptions, baseQuery) {
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
                  body: { ManualLoginInfo },
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
          const { role_id } = JwtUtil.decodeToken(token);
          if (role_id !== "1") {
            return {
              data: {
                isError: true,
                message: "You're not the Customer! Please use another web",
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
  }),
});

export const { useLoginMutation } = authApi;
