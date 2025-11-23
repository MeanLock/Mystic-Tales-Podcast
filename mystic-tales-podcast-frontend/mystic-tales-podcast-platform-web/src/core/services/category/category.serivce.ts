import { appApi } from "@/core/api/appApi";
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
  }),
});

export const { useGetCategoriesQuery } = categoryApi;
