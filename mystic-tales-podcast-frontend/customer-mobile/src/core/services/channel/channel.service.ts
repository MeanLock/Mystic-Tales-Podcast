import { appApi } from "../../api/appApi";
import { Channel, ChannelDetails } from "../../types/channel.type";
import { SubscriptionDetails } from "../../types/subscription.type";

const channelApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getChannelListFromPodcaster: build.query<
      { ChannelList: Channel[] },
      { podcasterId: number }
    >({
      query: ({ podcasterId }) => ({
        url: `/api/podcast-service/api/channels?podcaster_id=${podcasterId}`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getChannelListByQueryKey: build.query<
      { ChannelList: Channel[] },
      { queryKey: string }
    >({
      query: ({ queryKey }) => ({
        url: `/api/podcast-service/api/channels?${queryKey}`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getChannelDetails: build.query<{Channel: ChannelDetails }, { ChannelId: string }>({
      query: ({ ChannelId }) => ({
        url: `/api/podcast-service/api/channels/${ChannelId}`,
        method: "GET",
        authMode: "hybrid",
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

    favoriteChannel: build.mutation<
      { Message: string },
      { PodcastChannelId: string }
    >({
      async queryFn({ PodcastChannelId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/podcast-service/api/channels/${PodcastChannelId}/favorite/true`,
                method: "POST",
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),

    unfavoriteChannel: build.mutation<
      { Message: string },
      { PodcastChannelId: string }
    >({
      async queryFn({ PodcastChannelId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/podcast-service/api/channels/${PodcastChannelId}/favorite/false`,
                method: "POST",
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as any };
      },
    }),

    getFavoritedChannels: build.query<
      {
        ChannelList: Channel[];
      },
      void
    >({
      query: () => ({
        url: `/api/podcast-service/api/channels/favorited`,
        method: "GET",
        authMode: "required",
      }),
    }),
  }),
});

export const {
  useGetChannelListFromPodcasterQuery,
  useGetChannelListByQueryKeyQuery,
  useGetChannelDetailsQuery,
  useGetActiveChannelSubscriptionQuery,
  useFavoriteChannelMutation,
  useUnfavoriteChannelMutation,
  useGetFavoritedChannelsQuery,
} = channelApi;
