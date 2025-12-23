// @ts-nocheck
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

type ListenResponse<T> = {
  isError: boolean;
  message: string;
  data: T | null;
};

export const playerApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    // Listen to an episode, get listen session and procedure
    // listenToEpisode: build.mutation<
    //   {
    //     ListenSession: ListenSessionEpisodes | null;
    //     ListenSessionProcedure: ListenSessionProcedure;
    //   },
    //   {
    //     PodcastEpisodeId: string;
    //     SourceType: "SpecifyShowEpisodes" | "SavedEpisodes";
    //     CurrentPodcastSubscriptionRegistrationBenefitList: CurrentPodcastSubscriptionRegistrationBenefit[];
    //     continue_listen_session_id?: string;
    //   }
    // >({
    //   query: ({
    //     PodcastEpisodeId,
    //     SourceType,
    //     CurrentPodcastSubscriptionRegistrationBenefitList,
    //     continue_listen_session_id,
    //   }) => ({
    //     url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}/listen${
    //       continue_listen_session_id
    //         ? `?continue_listen_session_id=${continue_listen_session_id}`
    //         : ""
    //     }`,
    //     method: "POST",
    //     authMode: "required",
    //     body: {
    //       SourceType,
    //       CurrentPodcastSubscriptionRegistrationBenefitList,
    //     },
    //     headers: {
    //       "X-DeviceInfo-Token": localStorage.getItem("device_info_token") || "",
    //     },
    //   }),
    //   invalidatesTags: ["Account"],
    // }),

    listenToEpisode: build.mutation<
      ListenResponse<{
        ListenSession: ListenSessionEpisodes | null;
        ListenSessionProcedure: ListenSessionProcedure;
      }>,
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
        _queryApi,
        _extraOptions,
        fetchWithBQ
      ) {
        try {
          const result = await fetchWithBQ({
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
              "X-DeviceInfo-Token":
                localStorage.getItem("device_info_token") || "",
            },
          });

          // ❌ HTTP error
          if (result.error) {
            console.log("listenToEpisode error:", result);
            const message = result.error.details.message;
            let formattedMessage = "";

            if (message.includes("no subscription registration")) {
              formattedMessage = "Unsubscribed";
            } else if (message.includes("not in Published status")) {
              formattedMessage = "Content is not available";
            } else if (
              message.includes("insufficient benefits - missing conditions:")
            ) {
              const parseMissingConditions = (msg: string): string[] => {
                const marker = "missing conditions:";
                const index = msg.indexOf(marker);

                if (index === -1) return [];

                return msg
                  .slice(index + marker.length)
                  .split(",")
                  .map((s) => s.trim())
                  .filter(Boolean);
              };
              const missingConditions = parseMissingConditions(message);
              formattedMessage = `Insufficient benefits: ${missingConditions.join(
                ", "
              )}`;
            }
            return {
              data: {
                isError: true,
                message:
                  (result.error as any)?.data?.message ||
                  "Listen episode failed",
                data: null,
              },
            };
          }

          // ✅ success
          console.log("listenToEpisode success:", result);
          return {
            data: {
              isError: false,
              message: "Listen episode success",
              data: result.data as {
                ListenSession: ListenSessionEpisodes | null;
                ListenSessionProcedure: ListenSessionProcedure;
              },
            },
          };
        } catch (e: any) {
          // ❌ runtime error
          return {
            data: {
              isError: true,
              message: e?.message || "Unexpected error",
              data: null,
            },
          };
        }
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
      invalidatesTags: ["Account"],
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
                  maxAttempts: 3,
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

    getEpisodeLatestSession: build.mutation<
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

    getBookingLatestSession: build.mutation<
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
  useGetBookingLatestSessionMutation,
  useGetEpisodeLatestSessionMutation,
} = playerApi;
