import { appApi } from "@/core/api/appApi";
import type {
  BookingDetailsFromAPI,
  BookingFromAPI,
  BookingProducingRequestDetails,
  CompletedBooking,
  CompletedBookingDetails,
  PodcastBookingTone,
  PodcastBuddyFromAPI,
} from "@/core/types/booking";


export type CreateBookingPayload = {
  BookingCreateInfo: {
    Title: string;
    Description: string;
    PodcastBuddyId: number;
    BookingRequirementInfo: {
      Name: string;
      Description: string;
      Order: number;
      PodcastBookingToneId: string;
    }[];
  };
  BookingRequirementFiles: File[];
};

export const bookingApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    create: build.mutation<{ Message: string }, { createBookingFormData: any }>(
      {
        async queryFn({ createBookingFormData }, api) {
          const result = await api
            .dispatch(
              appApi.endpoints.kickoffThenWait.initiate({
                kickoff: {
                  url: "/api/booking-management-service/api/bookings",
                  method: "POST",
                  body: createBookingFormData,
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
      }
    ),
    getBookings: build.query<{ BookingList: BookingFromAPI[] }, void>({
      query: () => ({
        url: "/api/booking-management-service/api/bookings/given",
        method: "GET",
        authMode: "required",
      }),
    }),
    getBookingDetail: build.query<
      { Booking: BookingDetailsFromAPI },
      { id: number }
    >({
      query: ({ id }) => ({
        url: `/api/booking-management-service/api/bookings/${id}`,
        method: "GET",
        authMode: "required",
      }),
    }),
    confirmAndDeposit: build.mutation<
      { Message: string },
      { BookingId: number; Amount: number }
    >({
      async queryFn({ BookingId, Amount }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/booking-management-service/api/bookings/${BookingId}/deposit`,
                method: "POST",
                body: { Amount },
                authMode: "required",
              },
              poll: {
                intervalMs: 1000,
                maxAttempts: 30,
              },
            })
          )
          .unwrap();
        return { data: result.data as any };
      },
    }),
    getBookingProducingRequestDetails: build.query<
      { BookingProducingRequest: BookingProducingRequestDetails },
      { BookingProducingRequestId: string }
    >({
      query: ({ BookingProducingRequestId }) => ({
        url: `/api/booking-management-service/api/producing-requests/${BookingProducingRequestId}`,
        method: "GET",
        authMode: "required",
      }),
    }),
    sendNewEditRequest: build.mutation<
      { Message: string },
      {
        BookingId: number;
        Note: string;
        DeadlineDayCount: number;
        BookingPodcastTrackIds: string[];
      }
    >({
      async queryFn(
        { BookingId, Note, DeadlineDayCount, BookingPodcastTrackIds },
        api
      ) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/booking-management-service/api/bookings/${BookingId}/producing-request`,
                method: "POST",
                body: {
                  BookingProducingRequestInfo: {
                    Note,
                    DeadlineDayCount,
                    BookingPodcastTrackIds,
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
        return { data: result.data as any };
      },
    }),

    cancelBookingManually: build.mutation<
      { Message: string },
      { BookingId: number; CancelReason: string }
    >({
      async queryFn({ BookingId, CancelReason }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/booking-management-service/api/bookings/${BookingId}/cancel`,
                method: "PUT",
                body: {
                  BookingCancelledReason: CancelReason,
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
        return { data: result.data as any };
      },
    }),

    createCancelBookingRequest: build.mutation<
      { Message: string },
      { BookingId: number; CancelReason: string }
    >({
      async queryFn({ BookingId, CancelReason }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/booking-management-service/api/bookings/${BookingId}/cancel-request`,
                method: "POST",
                body: {
                  BookingCancelInfo: {
                    BookingManualCancelledReason: CancelReason,
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
        return { data: result.data as any };
      },
    }),

    // getPodcastBuddies: build.query<
    //   {
    //     PodcastBuddyList: {
    //       PodcastBuddyProfile: PodcasterProfile;
    //       ReviewList: PodcasterReviewAPI[];
    //     }[];
    //   },
    //   void
    // >({
    //   query: () => ({
    //     url: "/api/booking-management-service/api/podcast-buddies/available-me",
    //     method: "GET",
    //     authMode: "required",
    //   }),
    // }),

    acceptBookingAndPayTheRest: build.mutation<
      { Message: string },
      { BookingId: number }
    >({
      async queryFn({ BookingId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/booking-management-service/api/bookings/${BookingId}/pay-the-rest`,
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

    getCompletedBookings: build.query<
      { BookingList: CompletedBooking[] },
      void
    >({
      query: () => ({
        url: "/api/booking-management-service/api/bookings/completed",
        method: "GET",
        authMode: "required",
      }),
    }),

    getCompletedBookingDetail: build.query<
      { BookingList: CompletedBookingDetails },
      { BookingId: number }
    >({
      query: ({ BookingId }) => ({
        url: `/api/booking-management-service/api/bookings/get-completed-booking/${BookingId}`,
        method: "GET",
        authMode: "required",
      }),
    }),

    // Lấy danh sách các Podcast Booking Tones
    getPodcastBookingTones: build.query<
      {
        PodcastBookingToneList: PodcastBookingTone[];
      },
      void
    >({
      query: () => ({
        url: "/api/booking-management-service/api/bookings/podcast-booking-tone",
        method: "GET",
        authMode: "required",
      }),
    }),

    // Lấy danh sách các Podcast Buddies theo Podcast Booking Tone
    getPodcastBuddiesByBookingTone: build.query<
      { PodcastBuddyList: PodcastBuddyFromAPI[] },
      { PodcastBookingToneId: string }
    >({
      query: ({ PodcastBookingToneId }) => ({
        url: `/api/booking-management-service/api/bookings/podcast-booking-tone/${PodcastBookingToneId}/podcast-buddy`,
        method: "GET",
        authMode: "required",
      }),
    }),

    // Lấy danh sách các booking tone của 1 podcast buddy
    getBookingTonesOfPodcastBuddy: build.query<
      {
        PodcastBookingToneList: PodcastBookingTone[];
      },
      { PodcastBuddyId: number }
    >({
      query: ({ PodcastBuddyId }) => ({
        url: `/api/booking-management-service/api/bookings/podcast-booking-tone/podcast-buddy/${PodcastBuddyId}`,
        method: "GET",
        authMode: "required",
      }),
    }),

    getManunalCancelReasonOptions: build.query<
      { OptionalManualCancelReasonList: string[] },
      void
    >({
      query: () => ({
        url: `/api/booking-management-service/api/bookings/optional-manual-cancel-reasons`,
        method: "GET",
        authMode: "required",
      }),
    }),
  }),
});

// Hooks
export const {
  useCreateMutation,
  useGetBookingsQuery,
  useGetBookingDetailQuery,
  useConfirmAndDepositMutation,
  useGetBookingProducingRequestDetailsQuery,
  useSendNewEditRequestMutation,
  useCancelBookingManuallyMutation,
  useCreateCancelBookingRequestMutation,
  useAcceptBookingAndPayTheRestMutation,
  useGetCompletedBookingsQuery,
  useGetCompletedBookingDetailQuery,
  useGetPodcastBookingTonesQuery,
  useGetPodcastBuddiesByBookingToneQuery,
  useLazyGetPodcastBuddiesByBookingToneQuery,
  useGetBookingTonesOfPodcastBuddyQuery,
  useGetManunalCancelReasonOptionsQuery,
} = bookingApi;
