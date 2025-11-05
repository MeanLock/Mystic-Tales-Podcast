import { appApi } from "@/core/api/appApi";
import type { AuthMode } from "@/core/types";

interface ListenAudioResponse {
  Token: string;
  PlaylistFileKey: string;
  PodcastEpisode: {
    Id: string;
    Name: string;
    MainImageFileKey: string;
  };
}

export const playerApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    listenToAudio: build.query<
      ListenAudioResponse,
      { audioId: string; authMode?: AuthMode }
    >({
      query: ({ audioId, authMode = "required" }) => ({
        url: `/api/podcast-service/api/episodes/${audioId}/listen`,
        method: "GET",
        authMode,
      }),
    }),

    // Some backends return the m3u8 content directly (text). Request as text to avoid JSON parsing errors.
    getPlayListFile: build.query<
      string,
      { playlistFileKey: string; authMode?: AuthMode }
    >({
      query: ({ playlistFileKey, authMode = "required" }) => ({
        url: `/api/podcast-service/api/episodes/hls-playlist/get-file-data/${playlistFileKey}`,
        method: "GET",
        authMode,
        responseHandler: "text",
      }),
    }),
  }),
});

// Hooks
export const {
  useListenToAudioQuery,
  useLazyListenToAudioQuery,
  useLazyGetPlayListFileQuery,
} = playerApi;
