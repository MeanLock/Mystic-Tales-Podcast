// src/services/authApi.ts
import { baseApi } from "../baseApi";
import { tokenStore } from "@/src/features/auth/tokenStore";
import { setCredentials } from "@/src/features/auth/authSlice";
import type { User } from "@/src/types/user";
import type { FetchArgs, FetchBaseQueryError } from "@reduxjs/toolkit/query";

export const authApi = baseApi.injectEndpoints({
  endpoints: (b) => ({
    // Login -> fetch /me -> trả ra { accessToken, refreshToken, user }
    login: b.mutation<
      { accessToken: string; refreshToken: string; user: User },
      { LoginInfo: { Email: string; Password: string } }
    >({
      // Dùng queryFn để tự điều phối nhiều request
      queryFn: async (body, api, _extra, baseQuery) => {
        // 1) Gọi LOGIN (public, skip auth)
        const loginReq: FetchArgs = {
          url: "/api/user-service/api/auth/login-manual",
          method: "POST",
          body,
          headers: { "x-skip-auth": "true" },
        };
        const loginRes = await baseQuery(loginReq);

        if ("error" in loginRes) {
          return { error: loginRes.error as FetchBaseQueryError };
        }

        const { accessToken, refreshToken } = loginRes.data as {
          accessToken: string;
          refreshToken: string;
        };

        // 2) Lưu token an toàn
        await tokenStore.setAccess(accessToken);
        await tokenStore.setRefresh(refreshToken);

        // 3) Gọi /me với token vừa nhận (override header)
        const meReq: FetchArgs = {
          url: "/api/user-service/api/auth/me",
          method: "GET",
          headers: { Authorization: `Bearer ${accessToken}` },
        };
        const meRes = await baseQuery(meReq);

        if ("error" in meRes) {
          return { error: meRes.error as FetchBaseQueryError };
        }

        const user = meRes.data as User;

        // 4) Cập nhật Redux RAM (để các call sau auto gắn Authorization)
        api.dispatch(setCredentials({ user, accessToken }));

        // 5) Trả dữ liệu hợp nhất cho component gọi
        return { data: { accessToken, refreshToken, user } };
      },
    }),
  }),
  overrideExisting: false,
});

export const { useLoginMutation } = authApi;
