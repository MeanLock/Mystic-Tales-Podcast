import { appApi } from "@/core/api/appApi";
import type { DiscoveryData } from "@/core/types/feed";

const feedApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getDiscoveryFeed: build.query<DiscoveryData, void>({
      query: () => ({
        url: "/api/podcast-service/api/misc/feed/podcast-contents/discovery",
        method: "GET",
        authMode: "hybrid",
      }),
    }),
  }),
});

export const { useGetDiscoveryFeedQuery } = feedApi;
