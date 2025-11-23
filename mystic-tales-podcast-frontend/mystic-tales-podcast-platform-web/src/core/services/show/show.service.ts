import { appApi } from "@/core/api/appApi";
import type { ChannelFromAPI } from "@/core/types/channel";
import type { ShowDetailsFromAPI, ShowFromAPI } from "@/core/types/show";
import type { SubscriptionDetails } from "@/core/types/subscription";

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
    getShowDetails: build.query<
      { Show: ShowDetailsFromAPI },
      { PodcastShowId: string }
    >({
      query: ({ PodcastShowId }) => ({
        url: `/api/podcast-service/api/shows/${PodcastShowId}`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getActiveShowSubscription: build.query<
      { PodcastSubscription: SubscriptionDetails },
      { ShowId: string }
    >({
      query: ({ ShowId }) => ({
        url: `/api/subscription-service/api/podcast-subscriptions/shows/${ShowId}/active-subscription`,
        method: "GET",
        authMode: "hybrid",
      }),
    }),
    ratingShow: build.mutation<
      { Message: string },
      {
        PodcastShowId: string;
        Title: string;
        Content: string;
        Rating: number;
      }
    >({
      async queryFn({ PodcastShowId, Title, Content, Rating }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/podcast-service/api/shows/${PodcastShowId}/podcast-show-reviews`,
                method: "POST",
                body: {
                  PodcastShowReviewCreateInfo: {
                    Title,
                    Content,
                    Rating,
                  },
                },
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result as { Message: string } };
      },
    }),
  }),
});

export const {
  useGetShowListFromPodcasterQuery,
  useGetShowListByQueryKeyQuery,
  useGetShowDetailsQuery,
  useGetActiveShowSubscriptionQuery,
  useRatingShowMutation,
} = showApi;
