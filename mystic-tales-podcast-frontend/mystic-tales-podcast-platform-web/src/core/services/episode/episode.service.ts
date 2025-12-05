import { appApi } from "@/core/api/appApi";
import type {
  EpisodeDetailsFromAPI,
  EpisodeFromAPI,
} from "@/core/types/episode";

export type ListenHistory = {
  PodcastEpisode: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    IsReleased: boolean;
    AudioLength: number;
  };
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  CreatedAt: string;
};

const episodeApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    saveEpisode: build.mutation<
      { Message: string },
      { PodcastEpisodeId: string; IsSave: boolean }
    >({
      async queryFn({ PodcastEpisodeId, IsSave }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}/save/${IsSave}`,
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
        return { data: result as { Message: string } };
      },
    }),
    getSavedEpisodes: build.query<
      {
        SavedEpisodes: EpisodeFromAPI[];
      },
      void
    >({
      query: () => ({
        url: `/api/podcast-service/api/episodes/saved`,
        method: "GET",
        authMode: "required",
      }),
    }),
    getEpisodeDetails: build.query<
      { Episode: EpisodeDetailsFromAPI },
      { PodcastEpisodeId: string }
    >({
      query: ({ PodcastEpisodeId }) => ({
        url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}`,
        method: "GET",
        authMode: "required",
      }),
    }),

    getListenHistory: build.query<{ PodcastEpisodeListenHistory: ListenHistory[] }, void>({
      query: () => ({
        url: `/api/podcast-service/api/episodes/listen-sessions/podcast-episode-listen-history`,
        method: "GET",
        authMode: "required",
      }),
    }),
  }),
});

export const {
  useSaveEpisodeMutation,
  useGetSavedEpisodesQuery,
  useGetEpisodeDetailsQuery,
  useGetListenHistoryQuery,
} = episodeApi;
