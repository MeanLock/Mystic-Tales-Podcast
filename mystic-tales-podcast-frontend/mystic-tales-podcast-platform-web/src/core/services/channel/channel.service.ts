import { appApi } from "@/core/api/appApi";
import type {
  ChannelDetailsFromApi,
  ChannelFromAPI,
} from "@/core/types/channel";
import type { SubscriptionDetails } from "@/core/types/subscription";

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
    getChannelDetails: build.query<
      ChannelDetailsFromApi,
      { ChannelId: string }
    >({
      query: ({ ChannelId }) => ({
        url: `/api/podcast-service/api/channels/${ChannelId}`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getActiveChannelSubscription: build.query<
      { PodcastSubscription: SubscriptionDetails },
      { ChannelId: string }
    >({
      query: ({ ChannelId }) => ({
        url: `/api/subscription-service/api/podcast-subscriptions/channels/${ChannelId}/active-subscription`,
        method: "GET",
        authMode: "hybrid",
      }),
    }),
  }),
});

export const {
  useGetChannelListFromPodcasterQuery,
  useGetChannelListByQueryKeyQuery,
  useGetChannelDetailsQuery,
  useGetActiveChannelSubscriptionQuery,
} = channelApi;
