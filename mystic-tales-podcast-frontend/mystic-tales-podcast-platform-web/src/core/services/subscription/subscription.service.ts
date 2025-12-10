import { appApi } from "@/core/api/appApi";
import type { ChannelFromAPI } from "@/core/types/channel";
import type { ShowFromAPI } from "@/core/types/show";
import type {
  PodcastSubscriptionRegistration,
  PodcastSubscriptionRegistrationDetails,
  PodcastSubscriptionRegistrationFromAPI,
  SubscriptionDetails,
} from "@/core/types/subscription";

type SubscriptionApiResponse = {
  PodcastSubscriptionRegistration?: {
    PodcastSubscriptionBenefitList?: SubscriptionBenefit[];
  };
};

export type SubscriptionBenefit = {
  Id: number;
  Name: string;
};

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
        url: `/api/subscription-service/api/podcast-subscriptions/shows/podcast-subscriptions-registrations`,
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

    getSubscriptionBenefitsMapListFromEpisodeId: build.query<
      {
        CurrentPodcastSubscriptionRegistrationBenefitList: SubscriptionBenefit[];
      },
      { PodcastEpisodeId: string }
    >({
      query: ({ PodcastEpisodeId }) => ({
        url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscriptions-registrations/episodes/${PodcastEpisodeId}`,
        method: "GET",
        authMode: "required",
      }),
      transformResponse: (response: SubscriptionApiResponse) => {
        console.log("API Response:", response);

        const benefits =
          response?.PodcastSubscriptionRegistration
            ?.PodcastSubscriptionBenefitList ?? [];

        console.log("Fetched Benefits:", benefits);

        return {
          CurrentPodcastSubscriptionRegistrationBenefitList: benefits.map(
            (b) => ({
              Id: b.Id,
              Name: b.Name,
            })
          ),
        };
      },
    }),

    getSubscribedContents: build.query<
      {
        PodcastChannelList: ChannelFromAPI[];
        PodcastShowList: ShowFromAPI[];
      },
      void
    >({
      query: () => ({
        url: `/api/subscription-service/api/podcast-subscriptions/subscribed-content`,
        method: "GET",
        authMode: "required",
      }),
    }),

    getCustomerRegistrations: build.query<
      {
        PodcastSubscriptionRegistrationList: PodcastSubscriptionRegistrationFromAPI[];
      },
      void
    >({
      query: () => ({
        url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscriptions-registrations/me`,
        method: "GET",
        authMode: "required",
      }),
    }),
    cancelSubscriptionRegistration: build.mutation<
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

    makeDecisionOnAcceptingNewestVersion: build.mutation<
      { Message: string },
      { PodcastSubscriptionRegistrationId: string; IsAccepted: boolean }
    >({
      async queryFn({ PodcastSubscriptionRegistrationId, IsAccepted }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscriptions-registrations/${PodcastSubscriptionRegistrationId}/accept-newest-version/${IsAccepted}`,
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

    getRegistrationDetails: build.query<
      {
        PodcastSubscriptionRegistration: PodcastSubscriptionRegistrationDetails;
      },
      { PodcastSubscriptionRegistrationId: string }
    >({
      query: ({ PodcastSubscriptionRegistrationId }) => ({
        url: `/api/subscription-service/api/podcast-subscriptions/podcast-subscriptions-registrations/${PodcastSubscriptionRegistrationId}`,
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
  useGetCustomerRegistrationInfoFromChannelQuery,
  useGetCustomerRegistrationInfoFromShowQuery,
  useGetSubscriptionBenefitsMapListFromEpisodeIdQuery,
  useGetSubscribedContentsQuery,
  useGetCustomerRegistrationsQuery,
  useCancelSubscriptionRegistrationMutation,
  useMakeDecisionOnAcceptingNewestVersionMutation,
  useGetRegistrationDetailsQuery,
  useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery,
} = subscriptionApi;
