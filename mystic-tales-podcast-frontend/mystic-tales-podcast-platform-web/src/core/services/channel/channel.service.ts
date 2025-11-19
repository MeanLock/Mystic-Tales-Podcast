import { appApi } from "@/core/api/appApi";
import type { ChannelFromAPI } from "@/core/types/channel";

const channelApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getChannelListFromPodcaster: build.query<
      { ChannelList: ChannelFromAPI[] },
      { podcasterId: number }
    >({
      query: ({ podcasterId }) => ({
        url: `/api/podcast-service/api/channels?podcasterId=${podcasterId}`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getChannelListByQueryKey: build.query<
      { ChannelList: ChannelFromAPI[] },
      { queryKey: string }
    >({
      query: ({ queryKey }) => ({
        url: `/api/podcast-service/api/channels?${queryKey}`,
        method: "GET",
        authMode: "public",
      }),
    }),
  }),
});

export const {
  useGetChannelListFromPodcasterQuery,
  useGetChannelListByQueryKeyQuery,
} = channelApi;
