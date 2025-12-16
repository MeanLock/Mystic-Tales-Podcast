import { appApi } from "@/core/api/appApi";

const fileApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    // Account
    getAccountPublicSource: build.query<
      { FileUrl: string },
      { FileKey: string }
    >({
      query: ({ FileKey }) => ({
        url: `/api/user-service/api/misc/public-source/get-file-url/${FileKey}`,
        method: "GET",
        authMode: "public",
      }),
      keepUnusedDataFor: 0,
    }),
    getTemplatePodcastBuddyCommitmentFile: build.query<
      { FileUrl: string },
      { fileEnum: string }
    >({
      query: ({ fileEnum }) => ({
        url: `/api/user-service/api/misc/public-source/podcaster-documents/${fileEnum}/get-file-url`,
        method: "GET",
        authMode: "required",
      }),
      keepUnusedDataFor: 0,
    }),
    getPodcastBuddyCommitmentFile: build.query<
      { FileUrl: string },
      { FileKey: string }
    >({
      query: ({ FileKey }) => ({
        url: `/api/user-service/api/accounts/buddy-commitment-document/get-file-url/${FileKey}`,
        method: "GET",
        authMode: "required",
      }),
      keepUnusedDataFor: 0,
    }),
    // ---------------------------------------
    // Bookings
    getBookingPublicSource: build.query<
      { FileUrl: string },
      { FileKey: string }
    >({
      query: ({ FileKey }) => ({
        url: `/api/booking-management-service/api/misc/public-source/get-file-url/${FileKey}`,
        method: "GET",
        authMode: "required",
      }),
      keepUnusedDataFor: 0,
    }),
    // ---------------------------------------
    // Podcast
    getPodcastPublicSource: build.query<
      { FileUrl: string },
      { FileKey: string }
    >({
      query: ({ FileKey }) => ({
        url: `/api/podcast-service/api/misc/public-source/get-file-url/${FileKey}`,
        method: "GET",
        authMode: "public",
      }),
      keepUnusedDataFor: 0,
    }),
    // ---------------------------------------
    // Category
    getCategoryPublicSource: build.query<
      { FileUrl: string },
      { FileKey: string }
    >({
      query: ({ FileKey }) => ({
        url: `/api/podcast-service/api/categories/podcast-categories/get-file-url/${FileKey}`,
        method: "GET",
        authMode: "public",
      }),
      keepUnusedDataFor: 0,
    }),
  }),
});

export const {
  useGetAccountPublicSourceQuery,
  useGetTemplatePodcastBuddyCommitmentFileQuery,
  useGetPodcastBuddyCommitmentFileQuery,
  useGetBookingPublicSourceQuery,
  useGetPodcastPublicSourceQuery,
  useGetCategoryPublicSourceQuery,
  useLazyGetBookingPublicSourceQuery,
  useLazyGetPodcastPublicSourceQuery,
  useLazyGetAccountPublicSourceQuery,
} = fileApi;

export default fileApi;
