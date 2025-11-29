import { appApi } from "@/core/api/appApi";
import type {
  BookingDetailsFromAPI,
  BookingFromAPI,
  BookingProducingRequestDetails,
  PodcastBuddyFromAPI,
  PodcastBuddyUI,
} from "@/core/types/booking";
import type {
  PodcasterProfile,
  PodcasterReviewAPI,
} from "@/core/types/podcaster";

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
        url: "/api/booking-management-service/api/bookings/me",
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

    getPodcastBuddies: build.query<
      {
        PodcastBuddyList: {
          PodcastBuddyProfile: PodcasterProfile;
          ReviewList: PodcasterReviewAPI[];
        }[];
      },
      void
    >({
      query: () => ({
        url: "/api/booking-management-service/api/podcast-buddies/available-me",
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
} = bookingApi;
