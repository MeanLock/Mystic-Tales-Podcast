import { appApi } from "@/core/api/appApi";
import type {
  CategoryFeedDataFromAPI,
  DiscoveryData,
  TrendingData,
} from "@/core/types/feed";

const feedApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getDiscoveryFeed: build.query<DiscoveryData, void>({
      query: () => ({
        url: "/api/podcast-service/api/misc/feed/podcast-contents/discovery",
        method: "GET",
        authMode: "hybrid",
      }),
      keepUnusedDataFor: 0, // keep data for 1 minute
    }),
    getTrendingFeed: build.query<TrendingData, void>({
      query: () => ({
        url: "/api/podcast-service/api/misc/feed/podcast-contents/trending",
        method: "GET",
        authMode: "hybrid",
      }),
      keepUnusedDataFor: 60, // keep data for 1 minute
    }),
    getCategoryFeed: build.query<
      CategoryFeedDataFromAPI,
      { PodcastCategoryId: string }
    >({
      query: ({ PodcastCategoryId }) => ({
        url: `/api/podcast-service/api/misc/feed/podcast-contents/podcast-categories/${PodcastCategoryId}`,
        method: "GET",
        authMode: "hybrid",
      }),
      keepUnusedDataFor: 0, // keep data for 1 minute
    }),
  }),
});

export const { useGetDiscoveryFeedQuery, useGetTrendingFeedQuery } = feedApi;
