import { appApi } from "@/core/api/appApi";
import type { SubscriptionDetails } from "@/core/types/subscription";

const subscriptionApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    // Lấy chi tiết subscription
    getSubscriptionDetails: build.query<
      { PodcastSubscription: SubscriptionDetails },
      { PodcastSubscriptionId: string }
    >({
      query: ({ PodcastSubscriptionId }) => ({
        url: `/api/subscription-service/api/podcast-subscriptions/${PodcastSubscriptionId}`,
        method: "GET",
        authMode: "public",
      }),
    }),

    // Customer subscribe 1 Show/Channel
    subscribePodcastSubscription: build.mutation<
      { Message: string },
      { PodcastSubscriptionId: string }
    >({
      async queryFn({ PodcastSubscriptionId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/subscription-service/api/podcast-subscriptions/${PodcastSubscriptionId}`,
                method: "POST",
                body: { PodcastSubscriptionId },
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

    // Customer unsubscribe 1 Show/Channel
    unsubscribePodcastSubscription: build.mutation<
      { Message: string },
      { PodcastSubscriptionRegistrationId: string }
    >({
      async queryFn({ PodcastSubscriptionRegistrationId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscription-registrations/${PodcastSubscriptionRegistrationId}/cancel`,
                method: "PUT",
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

    // Lấy danh sách subscription của user với Channel
    getUserChannelSubscriptions: build.query<
      { ChannelSubscriptionRegistrationList: any[] },
      void
    >({
      query: () => ({
        url: `/api/subscription-service/api/podcast-subscriptions/channels/podcast-subscription-registrations`,
        method: "GET",
        authMode: "required",
      }),
    }),

    // Lấy danh sách subscription của user với Show
    getUserShowSubscriptions: build.query<
      { ShowSubscriptionRegistrationList: any[] },
      void
    >({
      query: () => ({
        url: `/api/subscription-service/api/podcast-subscriptions/shows/podcast-subscription-registrations`,
        method: "GET",
        authMode: "required",
      }),
    }),
  }),
});

export const {
  useGetSubscriptionDetailsQuery,
  useSubscribePodcastSubscriptionMutation,
  useUnsubscribePodcastSubscriptionMutation,
} = subscriptionApi;
