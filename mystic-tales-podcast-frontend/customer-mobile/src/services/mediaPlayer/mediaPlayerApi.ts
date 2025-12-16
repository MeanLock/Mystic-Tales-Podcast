import { baseApi } from "../baseApi";

export const mediaPlayerApi = baseApi.injectEndpoints({
  endpoints: (b) => ({
    getAudio: b.query<{ PlaylistUrl: string }, string>({
      query: (MainFileKey: string) => ({
        url: `/api/user-service/api/misc/file-source/get-audio-file-streaming-url/${MainFileKey}`,
        method: "GET",
      }),
      transformResponse: (response: { PlaylistUrl: string }) => response,
      // Cache for 5 minutes - adjust as needed
      keepUnusedDataFor: 300,
    }),
  }),
});

// Export the generated hooks for use in components
export const { useGetAudioQuery } = mediaPlayerApi;
