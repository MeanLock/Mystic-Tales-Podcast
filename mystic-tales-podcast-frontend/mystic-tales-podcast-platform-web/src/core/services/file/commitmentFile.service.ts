import { appApi } from "@/core/api/appApi";

export const commitmentFileApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getBasicFile: build.query<ArrayBuffer, void>({
      query: () => ({
        url: `/api/user-service/api/accounts/buddy-commitment-document/fill-podcaster-apply-info/get-file-content`,
        method: "GET",
        authMode: "required",
        responseHandler: async (response: Response): Promise<ArrayBuffer> => {
          return await response.arrayBuffer();
        },
      }),
    }),
    uploadSignImage: build.mutation<ArrayBuffer, { formData: FormData }>({
      query: ({ formData }) => ({
        url: `/api/user-service/api/accounts/buddy-commitment-document/sign/get-file-content`,
        method: "POST",
        authMode: "required",
        body: formData,
        responseHandler: async (response: Response): Promise<ArrayBuffer> => {
          return await response.arrayBuffer();
        },
      }),
    }),
  }),
});

export const { useGetBasicFileQuery, useUploadSignImageMutation } =
  commitmentFileApi;
