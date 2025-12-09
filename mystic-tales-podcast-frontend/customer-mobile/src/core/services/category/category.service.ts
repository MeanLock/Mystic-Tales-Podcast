import type { CategoryFeedData } from "@/src/core/types/feed.type";
import type { PodcastCategoryWithImageFromAPI } from "@/src/core/types/podcastCategory.type";
import { appApi } from "../../api/appApi";

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
      CategoryFeedData,
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

export const { useGetCategoriesQuery, useGetCategoryFeedDataQuery } =
  categoryApi;
