import { appApi } from "@/core/api/appApi";

export const commitmentFileApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getBasicFile: build.query<{ FileUrl: string }, void>({
      query: () => ({
        url: `/api/user-service/api/misc/public-source/podcaster-documents/MainBuddyCommitmentDocumentTemplate/get-file-url`,
        method: "GET",
        authMode: "required",
      }),
    }),
  }),
});

export const { useGetBasicFileQuery } = commitmentFileApi;
