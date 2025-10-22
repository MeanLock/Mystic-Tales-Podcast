// src/services/authApi.ts
import { baseApi } from "../baseApi";
import { tokenStore } from "@/src/features/auth/tokenStore";
import { User } from "@/src/types/user";

export const accountApi = baseApi.injectEndpoints({
  endpoints: (b) => ({
    // getMe: b.query<User, void>({
    //     query: () => ({
    //         url: "/api/user-service/api/auth/me",
    //         method: "GET",
    //     }),
    // }),
  }),
});

// export const { useGetMeQuery } = accountApi;
