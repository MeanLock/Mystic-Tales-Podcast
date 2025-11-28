import { appApi } from "@/core/api/appApi";
import type { CategoryFeedDataFromAPI } from "@/core/types/feed";
import type { PodcastCategoryWithImageFromAPI } from "@/core/types/podcastCategory";

const categoryApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getCategories: build.query<
      { PodcastCategoryList: PodcastCategoryWithImageFromAPI[] },
      void
    >({
      query: () => ({
        url: `/api/podcast-service/api/categories/podcast-categories`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getCategoryFeedData: build.query<
      CategoryFeedDataFromAPI,
      { PodcastCategoryId: number }
    >({
      query: ({ PodcastCategoryId }) => ({
        url: `/api/podcast-service/api/misc/feed/podcast-contents/podcast-categories/${PodcastCategoryId}`,
        method: "GET",
        authMode: "hybrid",
      }),
    }),
  }),
});

export const { useGetCategoriesQuery, useGetCategoryFeedDataQuery } = categoryApi;
