import { appApi } from "../../api/appApi";
import type {
  CategoryFeedData,
  DiscoveryData,
  TrendingData,
} from "../../types/feed.type";

const feedApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getDiscoveryFeed: build.query<DiscoveryData, void>({
      query: () => ({
        url: "/api/podcast-service/api/misc/feed/podcast-contents/discovery",
        method: "GET",
        authMode: "hybrid",
      }),
    }),
    getTrendingFeed: build.query<TrendingData, void>({
      query: () => ({
        url: "/api/podcast-service/api/misc/feed/podcast-contents/trending",
        method: "GET",
        authMode: "hybrid",
      }),
    }),
    getCategoryFeed: build.query<
      CategoryFeedData,
      { PodcastCategoryId: string }
    >({
      query: ({ PodcastCategoryId }) => ({
        url: `/api/podcast-service/api/misc/feed/podcast-contents/podcast-categories/${PodcastCategoryId}`,
        method: "GET",
        authMode: "hybrid",
      }),
    }),
  }),
});

export const { useGetDiscoveryFeedQuery, useGetTrendingFeedQuery } = feedApi;
