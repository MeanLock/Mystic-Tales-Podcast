import type {
  PodcastBuddyDetails,
  PodcasterDetailsFromAPI,
  PodcasterFromApi,
} from "@/src/core/types/podcaster.type";
import { appApi } from "../../api/appApi";

const podcasterApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getPodcasters: build.query<
      { PodcasterList: PodcasterFromApi[] },
      { queryKey: string }
    >({
      query: ({ queryKey }) => ({
        url: `/api/user-service/api/accounts/customer/podcasters?${queryKey}`,
        method: "GET",
        authMode: "public",
      }),
    }),
    getPodcasterDetails: build.query<
      PodcasterDetailsFromAPI,
      { podcasterId: number }
    >({
      query: ({ podcasterId }) => ({
        url: `/api/user-service/api/accounts/customer/podcasters/${podcasterId}`,
        method: "GET",
        authMode: "hybrid",
      }),
    }),
    getFollowedPodcasters: build.query<
      { FollowedPodcasterList: PodcasterFromApi[] },
      void
    >({
      query: () => ({
        url: `/api/user-service/api/accounts/followed-podcasters`,
        method: "GET",
        authMode: "required",
      }),
    }),
    followPodcaster: build.mutation<
      { Message: string },
      { PodcasterId: number }
    >({
      async queryFn({ PodcasterId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/user-service/api/accounts/${PodcasterId}/follow/true`,
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
    unFollowPodcaster: build.mutation<
      { Message: string },
      { PodcasterId: number }
    >({
      async queryFn({ PodcasterId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/user-service/api/accounts/${PodcasterId}/follow/false`,
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
    getPodcastBuddyDetails: build.query<
      PodcastBuddyDetails,
      { AccountId: number }
    >({
      query: ({ AccountId }) => ({
        url: `/api/user-service/api/accounts/podcast-buddies/${AccountId}`,
        method: "GET",
        authMode: "required",
      }),
    }),
  }),
});

export const {
  useGetPodcasterDetailsQuery,
  useGetFollowedPodcastersQuery,
  useFollowPodcasterMutation,
  useUnFollowPodcasterMutation,
  useGetPodcastersQuery,
  useGetPodcastBuddyDetailsQuery,
} = podcasterApi;
