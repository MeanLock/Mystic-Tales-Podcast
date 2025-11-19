import { appApi } from "@/core/api/appApi";
import type { ChannelFromAPI } from "@/core/types/channel";
import type { ShowFromAPI } from "@/core/types/show";

const showApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getShowListFromPodcaster: build.query<
      { ShowList: ShowFromAPI[] },
      { podcasterId: number }
    >({
      query: ({ podcasterId }) => ({
        url: `/api/podcast-service/api/shows?podcasterId=${podcasterId}`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getShowListByQueryKey: build.query<
      { ShowList: ShowFromAPI[] },
      { queryKey: string }
    >({
      query: ({ queryKey }) => ({
        url: `/api/podcast-service/api/shows?${queryKey}`,
        method: "GET",
        authMode: "public",
      }),
    }),
  }),
});

export const {
  useGetShowListFromPodcasterQuery,
  useGetShowListByQueryKeyQuery,
} = showApi;
