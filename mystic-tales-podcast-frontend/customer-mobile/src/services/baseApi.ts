import {
  createApi,
  fetchBaseQuery,
  type FetchArgs,
} from "@reduxjs/toolkit/query/react";
import type { RootState } from "@/src/store/store";

export const baseApi = createApi({
  reducerPath: "api",
  tagTypes: ["FileUrl", "Episodes"] as const,
  baseQuery: fetchBaseQuery({
    baseUrl: "https://api.yourdomain.com",
    prepareHeaders: (headers, { getState }) => {
      // Nếu endpoint đã set Authorization → giữ nguyên (override cho call đặc biệt)
      if (headers.has("authorization")) return headers;
      // Skip auth cho public endpoint
      if (headers.get("x-skip-auth") === "true") {
        headers.delete("x-skip-auth");
        return headers;
      }
      // Auto: dùng token trong RAM (Redux)
      const token = (getState() as RootState).auth.accessToken;
      if (token) headers.set("authorization", `Bearer ${token}`);
      return headers;
    },
  }),
  endpoints: () => ({}),
});
