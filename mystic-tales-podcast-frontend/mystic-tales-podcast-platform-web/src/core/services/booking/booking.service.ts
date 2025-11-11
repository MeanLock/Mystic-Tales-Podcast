import { appApi } from "@/core/api/appApi";
import type { BookingDetailsFromAPI, BookingDetailsUI, BookingFromAPI } from "@/core/types/booking";

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
          return { data: result.data as any };
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
    getBookingDetail: build.query<{ Booking: BookingDetailsUI }, { id: number }>({
      query: ({ id }) => ({
        url: `/api/booking-management-service/api/bookings/${id}`,
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
} = bookingApi;
