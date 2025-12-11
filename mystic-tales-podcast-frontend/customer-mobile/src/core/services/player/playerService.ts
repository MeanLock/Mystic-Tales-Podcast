import type { ApiErrorModel } from "@/src/core/types/saga.types";
import type {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
  ListenSessionProcedure,
} from "@/src/core/types/audio.type";
import { appApi } from "../../api/appApi";
import * as SecureStore from "expo-secure-store";

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
      async queryFn(
        {
          PodcastEpisodeId,
          SourceType,
          CurrentPodcastSubscriptionRegistrationBenefitList,
          continue_listen_session_id,
        },
        _api,
        _extraOptions,
        baseQuery
      ) {
        const deviceToken =
          (await SecureStore.getItemAsync("device_info_token")) || "";
        const result = await baseQuery({
          url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}/listen${
            continue_listen_session_id
              ? `?continue_listen_session_id=${continue_listen_session_id}`
              : ""
          }`,
          method: "POST",
          authMode: "required",
          body: {
            SourceType,
            CurrentPodcastSubscriptionRegistrationBenefitList,
          },
          headers: {
            "X-DeviceInfo-Token": deviceToken,
          },
        });
        if (result.error) return { error: result.error as any };
        return { data: result.data as any };
      },
      invalidatesTags: ["Account"],
    }),

    // Listen to a booking track, get listen session and procedure
    listenToBookingTrack: build.mutation<
      {
        ListenSession: ListenSessionBookingTracks | null;
        ListenSessionProcedure: ListenSessionProcedure;
      },
      {
        BookingId: number;
        BookingPodcastTrackId: string;
      }
    >({
      async queryFn(
        { BookingId, BookingPodcastTrackId },
        _api,
        _extra,
        baseQuery
      ) {
        const deviceToken =
          (await SecureStore.getItemAsync("device_info_token")) || "";
        const result = await baseQuery({
          url: `/api/booking-management-service/api/bookings/${BookingId}/booking-podcast-tracks/${BookingPodcastTrackId}/listen`,
          method: "POST",
          authMode: "required",
          body: {
            SourceType: "BookingProducingTracks",
          },
          headers: {
            "X-DeviceInfo-Token": deviceToken,
          },
        });
        if (result.error) return { error: result.error as any };
        return { data: result.data as any };
      },
      invalidatesTags: ["Account"],
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
      async queryFn(
        {
          ListenSessionNavigateType,
          ListenSessionId,
          ListenSessionProcedureId,
          CurrentPodcastSubscriptionRegistrationBenefitList,
        },
        _api,
        _extra,
        baseQuery
      ) {
        const deviceToken =
          (await SecureStore.getItemAsync("device_info_token")) || "";
        const result = await baseQuery({
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
            "X-DeviceInfo-Token": deviceToken,
          },
        });
        if (result.error) return { error: result.error as any };
        return { data: result.data as any };
      },
      invalidatesTags: ["Account"],
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
      async queryFn(
        {
          ListenSessionNavigateType,
          ListenSessionId,
          ListenSessionProcedureId,
        },
        _api,
        _extra,
        baseQuery
      ) {
        const deviceToken =
          (await SecureStore.getItemAsync("device_info_token")) || "";
        const result = await baseQuery({
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
            "X-DeviceInfo-Token": deviceToken,
          },
        });
        if (result.error) return { error: result.error as any };
        return { data: result.data as any };
      },
      invalidatesTags: ["Account"],
    }),

    // Update episode listen session last duration seconds
    updateEpisodeLastDuration: build.mutation<
      { Message: string } | undefined,
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
        return { data: result as { Message: string } | undefined };
      },
    }),

    // Update booking track listen session last duration seconds
    updateBookingTrackLastDuration: build.mutation<
      { Message: string } | undefined,
      {
        BookingPodcastTrackListenSessionId: string;
        LastListenDurationSeconds: number;
      }
    >({
      async queryFn(
        { BookingPodcastTrackListenSessionId, LastListenDurationSeconds },
        api
      ) {
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
        return { data: result as { Message: string } | undefined };
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
      async queryFn(_arg, _api, _extra, baseQuery) {
        const deviceToken =
          (await SecureStore.getItemAsync("device_info_token")) || "";
        const result = await baseQuery({
          url: `/api/podcast-service/api/episodes/listen-sessions/latest`,
          method: "GET",
          authMode: "required",
          headers: {
            "X-DeviceInfo-Token": deviceToken,
          },
        });
        if (result.error) return { error: result.error as any };
        return { data: result.data as any };
      },
    }),

    getBookingLatestSession: build.query<
      {
        ListenSession: ListenSessionBookingTracks | null;
        ListenSessionProcedure: ListenSessionProcedure | null;
      },
      void
    >({
      async queryFn(_arg, _api, _extra, baseQuery) {
        const deviceToken =
          (await SecureStore.getItemAsync("device_info_token")) || "";
        const result = await baseQuery({
          url: `/api/booking-management-service/api/bookings/listen-sessions/latest`,
          method: "GET",
          authMode: "required",
          headers: {
            "X-DeviceInfo-Token": deviceToken,
          },
        });
        if (result.error) return { error: result.error as any };
        return { data: result.data as any };
      },
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
  useLazyGetBookingLatestSessionQuery,
  useLazyGetEpisodeLatestSessionQuery,
} = playerApi;
