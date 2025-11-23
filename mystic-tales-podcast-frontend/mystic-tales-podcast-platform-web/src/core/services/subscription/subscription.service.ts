import { appApi } from "@/core/api/appApi";
import type {
  PodcastSubscriptionRegistration,
  SubscriptionDetails,
} from "@/core/types/subscription";
import { url } from "zod";

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
      { PodcastSubscriptionId: number; CycleTypeId: number }
    >({
      async queryFn({ PodcastSubscriptionId, CycleTypeId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/subscription-service/api/podcast-subscriptions/${PodcastSubscriptionId}`,
                method: "POST",
                body: { SubscriptionCycleTypeId: CycleTypeId },
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
                url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscriptions-registrations/${PodcastSubscriptionRegistrationId}/cancel`,
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

    // Lấy danh sách subscription của user với Channels
    getUserChannelSubscriptions: build.query<
      {
        ChannelSubscriptionRegistrationList: PodcastSubscriptionRegistration[];
      },
      void
    >({
      query: () => ({
        url: `/api/subscription-service/api/podcast-subscriptions/channels/podcast-subscriptions-registrations`,
        method: "GET",
        authMode: "required",
      }),
    }),

    // Lấy danh sách subscription của user với Shows
    getUserShowSubscriptions: build.query<
      { ShowSubscriptionRegistrationList: PodcastSubscriptionRegistration[] },
      void
    >({
      query: () => ({
        url: `/api/subscription-service/api/podcast-subscriptions/shows/podcast-subscription-registrations`,
        method: "GET",
        authMode: "required",
      }),
    }),

    // Lấy thông tin đăng ký của Customer so với Channel đó
    getCustomerRegistrationInfoFromChannel: build.query<
      {
        PodcastSubscriptionRegistration: PodcastSubscriptionRegistration | null;
      },
      { PodcastChannelId: string }
    >({
      query: ({ PodcastChannelId }) => ({
        url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscriptions-registrations/channels/${PodcastChannelId}`,
        authMode: "required",
        method: "GET",
      }),
    }),

    // Lấy thông tin đăng ký của Customer so với Show đó
    getCustomerRegistrationInfoFromShow: build.query<
      {
        PodcastSubscriptionRegistration: PodcastSubscriptionRegistration | null;
      },
      { PodcastShowId: string }
    >({
      query: ({ PodcastShowId }) => ({
        url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscriptions-registrations/shows/${PodcastShowId}`,
        authMode: "required",
        method: "GET",
      }),
    }),
  }),
});

export const {
  useGetSubscriptionDetailsQuery,
  useSubscribePodcastSubscriptionMutation,
  useUnsubscribePodcastSubscriptionMutation,
  useGetCustomerRegistrationInfoFromChannelQuery,
  useGetCustomerRegistrationInfoFromShowQuery,
} = subscriptionApi;
