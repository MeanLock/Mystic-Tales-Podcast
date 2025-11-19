import { appApi } from "@/core/api/appApi";
import type { PodcasterDetailsFromAPI } from "@/core/types/podcaster";

const podcasterApi = appApi.injectEndpoints({
  endpoints: (build) => ({
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
      { PodcasterList: PodcasterDetailsFromAPI[] },
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
  }),
});

export const {
  useGetPodcasterDetailsQuery,
  useGetFollowedPodcastersQuery,
  useFollowPodcasterMutation,
  useUnFollowPodcasterMutation,
} = podcasterApi;
