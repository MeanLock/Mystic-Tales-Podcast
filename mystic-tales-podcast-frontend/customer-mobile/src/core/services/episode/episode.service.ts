import { appApi } from "../../api/appApi";
import { Episode, EpisodeDetails } from "../../types/episode.type";

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
        SavedEpisodes: Episode[];
      },
      void
    >({
      query: () => ({
        url: `/api/podcast-service/api/episodes/saved`,
        method: "GET",
        authMode: "required",
      }),
    }),

    getEpisodeDetails: build.query<{Episode: EpisodeDetails}, {PodcastEpisodeId: string}>({
      query: ({PodcastEpisodeId}) => ({
        url: `/api/podcast-service/api/episodes/${PodcastEpisodeId}`,
        method: "GET",
        authMode: "hybrid",
      }),
    }),
  }),
});

export const { useSaveEpisodeMutation, useGetSavedEpisodesQuery, useGetEpisodeDetailsQuery } = episodeApi;
