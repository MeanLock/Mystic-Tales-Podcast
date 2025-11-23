import { appApi } from "@/core/api/appApi";
import type { ApiErrorModel, AuthMode } from "@/core/types";
import type {
  ListenSession,
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "@/core/types/audio";

interface ListenAudioResponse {
  Token: string;
  PlaylistFileKey: string;
  PodcastEpisode: {
    Id: string;
    Name: string;
    MainImageFileKey: string;
  };
}

type CurrentPodcastSubscriptionRegistrationBenefit = {
  Id: number;
  Name: string;
};

export const playerApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    // Listen to an episode, get listen session and procedure
    listenToEpisode: build.mutation<
      {
        ListenSession: ListenSessionEpisodes | null;
        ListenSessionProcedure: ListenSessionProcedure;
      },
      {
        PodcastEpisodeId: string;
        SourceType: "SpecifyShowEpisodes" | "SavedEpisodes";
        CurrentPodcastSubscriptionRegistrationBenefitList: CurrentPodcastSubscriptionRegistrationBenefit[];
      }
    >({
      query: ({
        PodcastEpisodeId,
        SourceType,
        CurrentPodcastSubscriptionRegistrationBenefitList,
      }) => {
        const deviceToken =
          localStorage.getItem("device_info_token") ||
          "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJEZXZpY2VJZCI6ImRhMjdiMjQxLTg1YWItNDI2OS05ZmE0LWY0NGQ4MWNkNjVhYyIsIlBsYXRmb3JtIjoid2ViIiwiT1NOYW1lIjoid2luZG93cyIsImV4cCI6Nzc2MzkwNDkzMH0.BCZR_7ETUKEZZFNhnD8wBZlKdo6QGs0Rxpeta6mQDaw";

        console.log("DeviceToken:", deviceToken);

        return {
          url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}/listen`,
          method: "POST",
          authMode: "required" as const,
          body: {
            SourceType,
            CurrentPodcastSubscriptionRegistrationBenefitList,
          },
          headers: {
            "X-DeviceInfo-Token": deviceToken,
          },
        };
      },
    }),

    // Listen to a booking track, get listen session and procedure
    listenToBookingTrack: build.mutation<
      {
        ListenSession: ListenSessionBookingTracks | null;
        ListenSessionProcedure: ListenSessionProcedure;
      },
      {
        BookingId: string;
        BookingPodcastTrackId: string;
      }
    >({
      query: ({ BookingId, BookingPodcastTrackId }) => ({
        url: `/api/booking-management-service/api/bookings/${BookingId}/booking-podcast-tracks/${BookingPodcastTrackId}/listen`,
        method: "POST",
        authMode: "required",
        body: {
          SourceType: "BookingProducingTracks",
        },
        headers: {
          "X-DeviceInfo-Token": localStorage.getItem("device_info_token") || "",
        },
      }),
    }),

    // Update episode listen session last duration seconds
    updateEpisodeLastDuration: build.mutation<
      { Message: string },
      {
        PodcastEpisodeListenSessionId: string;
        LastListenDurationSeconds: number;
        CurrentPodcastSubscriptionRegistrationBenefitList: CurrentPodcastSubscriptionRegistrationBenefit[];
      }
    >({
      async queryFn(
        {
          PodcastEpisodeListenSessionId,
          LastListenDurationSeconds,
          CurrentPodcastSubscriptionRegistrationBenefitList,
        },
        api
      ) {
        try {
          const result = await api
            .dispatch(
              appApi.endpoints.kickoffThenWait.initiate({
                kickoff: {
                  url: `/api/podcast-service/api/episodes/listen-sessions/${PodcastEpisodeListenSessionId}/last-duration-seconds/${LastListenDurationSeconds}`,
                  method: "PUT",
                  body: {
                    CurrentPodcastSubscriptionRegistrationBenefitList,
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

          // Saga success -> trả data
          return { data: result as { Message: string } };
        } catch (e: any) {
          // e ở đây chính là ApiErrorModel mà kickoffThenWait trả ra (hoặc throw từ pollSagaResult)
          const apiErr: ApiErrorModel = e?.kind
            ? e
            : {
                kind: "UNKNOWN",
                message: e?.message ?? "Unknown saga error",
                details: e,
              };

          return { error: apiErr };
        }
      },
    }),

    // Update booking track listen session last duration seconds
    updateBookingTrackLastDuration: build.mutation<
      { Message: string },
      {
        BookingPodcastTrackListenSessionId: string;
        LastListenDurationSeconds: number;
      }
    >({
      async queryFn(
        { BookingPodcastTrackListenSessionId, LastListenDurationSeconds },
        api
      ) {
        try {
          const result = await api
            .dispatch(
              appApi.endpoints.kickoffThenWait.initiate({
                kickoff: {
                  url: `/api/booking-management-service/api/bookings/listen-sessions/${BookingPodcastTrackListenSessionId}/last-duration-seconds/${LastListenDurationSeconds}`,
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
          return { data: result as any };
        } catch (e: any) {
          // e ở đây chính là ApiErrorModel mà kickoffThenWait trả ra (hoặc throw từ pollSagaResult)
          const apiErr: ApiErrorModel = e?.kind
            ? e
            : {
                kind: "UNKNOWN",
                message: e?.message ?? "Unknown saga error",
                details: e,
              };

          return { error: apiErr };
        }
      },
    }),

    updatePlayMode: build.mutation<
      { Message: string },
      {
        PlayOrderMode: "Sequential" | "Random";
        IsAutoPlay: boolean;
        CustomerListenSessionProcedureId: string;
      }
    >({
      query: ({
        PlayOrderMode,
        IsAutoPlay,
        CustomerListenSessionProcedureId,
      }) => ({
        url: `/api/user-service/api/accounts/customer-listen-session-procedures/${CustomerListenSessionProcedureId}`,
        method: "PUT",
        authMode: "required",
        body: {
          CustomerListenSessionProcedureUpdateInfo: {
            PlayOrderMode: PlayOrderMode,
            IsAutoPlay: IsAutoPlay,
          },
        },
      }),
    }),
  }),
});

// Hooks
export const {
  useListenToEpisodeMutation,
  useListenToBookingTrackMutation,
  useUpdateEpisodeLastDurationMutation,
  useUpdateBookingTrackLastDurationMutation,
  useUpdatePlayModeMutation,
} = playerApi;
