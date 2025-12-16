import { appApi } from "@/src/core/api/appApi/index";

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
  useLazyGetPodcastPublicSourceQuery
} = fileApi;

export default fileApi;
