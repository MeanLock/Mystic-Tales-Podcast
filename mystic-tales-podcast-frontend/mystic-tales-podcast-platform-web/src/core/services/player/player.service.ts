import { appApi } from "@/core/api/appApi";
import type { ApiErrorModel } from "@/core/types";
import type {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "@/core/types/audio";

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
        continue_listen_session_id?: string;
      }
    >({
      query: ({
        PodcastEpisodeId,
        SourceType,
        CurrentPodcastSubscriptionRegistrationBenefitList,
        continue_listen_session_id,
      }) => {
        const deviceToken =
          localStorage.getItem("device_info_token") ||
          "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJEZXZpY2VJZCI6ImRhMjdiMjQxLTg1YWItNDI2OS05ZmE0LWY0NGQ4MWNkNjVhYyIsIlBsYXRmb3JtIjoid2ViIiwiT1NOYW1lIjoid2luZG93cyIsImV4cCI6Nzc2MzkwNDkzMH0.BCZR_7ETUKEZZFNhnD8wBZlKdo6QGs0Rxpeta6mQDaw";

        console.log("DeviceToken:", deviceToken);

        return {
          url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}/listen${
            continue_listen_session_id
              ? `?continue_listen_session_id=${continue_listen_session_id}`
              : ""
          }`,
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

    // Navigate listen to next/previous episode in procedure
    navigateEpisodeInProcedure: build.mutation<
      {
        ListenSession: ListenSessionEpisodes | null;
        ListenSessionProcedure: ListenSessionProcedure;
      },
      {
        ListenSessionNavigateType: "Next" | "Previous";
        ListenSessionId: string;
        ListenSessionProcedureId: string;
        CurrentPodcastSubscriptionRegistrationBenefitList:
          | CurrentPodcastSubscriptionRegistrationBenefit[]
          | null;
      }
    >({
      query: ({
        ListenSessionNavigateType,
        ListenSessionId,
        ListenSessionProcedureId,
        CurrentPodcastSubscriptionRegistrationBenefitList,
      }) => ({
        url: `/api/podcast-service/api/episodes/listen-sessions/navigate?listen_session_navigate_type=${ListenSessionNavigateType}`,
        method: "POST",
        authMode: "required",
        body: {
          CurrentListenSession: {
            ListenSessionId,
            ListenSessionProcedureId,
          },
          CurrentPodcastSubscriptionRegistrationBenefitList,
        },
        headers: {
          "X-DeviceInfo-Token": localStorage.getItem("device_info_token") || "",
        },
      }),
    }),

    // Navigate listen to next/previous booking tracks in procedure
    navigateBookingTrackInProcedure: build.mutation<
      {
        ListenSession: ListenSessionBookingTracks | null;
        ListenSessionProcedure: ListenSessionProcedure;
      },
      {
        ListenSessionNavigateType: "Next" | "Previous";
        ListenSessionId: string;
        ListenSessionProcedureId: string;
        CurrentPodcastSubscriptionRegistrationBenefitList:
          | CurrentPodcastSubscriptionRegistrationBenefit[]
          | null;
      }
    >({
      query: ({
        ListenSessionNavigateType,
        ListenSessionId,
        ListenSessionProcedureId,
      }) => ({
        url: `/api/booking-management-service/api/bookings/listen-sessions/navigate?listen_session_navigate_type=${ListenSessionNavigateType}`,
        method: "POST",
        authMode: "required",
        body: {
          CurrentListenSession: {
            ListenSessionId,
            ListenSessionProcedureId,
          },
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
        CurrentPodcastSubscriptionRegistrationBenefitList:
          | CurrentPodcastSubscriptionRegistrationBenefit[]
          | null;
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

    getEpisodeLatestSession: build.query<
      {
        ListenSession: ListenSessionEpisodes | null;
        ListenSessionProcedure: ListenSessionProcedure | null;
      },
      void
    >({
      query: () => ({
        url: `/api/podcast-service/api/episodes/listen-sessions/latest`,
        method: "GET",
        authMode: "required",
        headers: {
          "X-DeviceInfo-Token": localStorage.getItem("device_info_token") || "",
        },
      }),
    }),

    getBookingLatestSession: build.query<
      {
        ListenSession: ListenSessionBookingTracks | null;
        ListenSessionProcedure: ListenSessionProcedure | null;
      },
      void
    >({
      query: () => ({
        url: `/api/booking-management-service/api/bookings/listen-sessions/latest`,
        method: "GET",
        authMode: "required",
        headers: {
          "X-DeviceInfo-Token": localStorage.getItem("device_info_token") || "",
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
  useNavigateBookingTrackInProcedureMutation,
  useNavigateEpisodeInProcedureMutation,
  useGetBookingLatestSessionQuery,
  useGetEpisodeLatestSessionQuery,
} = playerApi;
