import { appApi } from "@/core/api/appApi";
import type { ReportType } from "@/core/types/report";

const reportApi = appApi.injectEndpoints({
  endpoints: (build) => ({
    getShowReportTypes: build.query<
      { ShowReportTypeList: ReportType[] },
      { PodcastShowId: string }
    >({
      query: ({ PodcastShowId }) => ({
        url: `/api/moderation-service/api/show-reports/shows/${PodcastShowId}/show-report-types`,
        method: "GET",
        authMode: "required",
      }),
    }),
    getEpisodeReportTypes: build.query<
      { EpisodeReportTypeList: ReportType[] },
      { PodcastEpisodeId: string }
    >({
      query: ({ PodcastEpisodeId }) => ({
        url: `/api/moderation-service/api/episode-reports/episodes/${PodcastEpisodeId}/episode-report-types`,
        method: "GET",
        authMode: "required",
      }),
    }),
    reportShow: build.mutation<
      { Message: string },
      { PodcastShowId: string; Content: string; ReportTypeId: number }
    >({
      async queryFn({ PodcastShowId, Content, ReportTypeId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/moderation-service/api/show-reports/${PodcastShowId}`,
                method: "POST",
                authMode: "required",
                body: {
                  ShowReportCreateInfo: {
                    Content: Content,
                    PodcastShowReportTypeId: ReportTypeId,
                  },
                },
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
    reportEpisode: build.mutation<
      { Message: string },
      { PodcastEpisodeId: string; Content: string; ReportTypeId: number }
    >({
      async queryFn({ PodcastEpisodeId, Content, ReportTypeId }, api) {
        const result = await api
          .dispatch(
            appApi.endpoints.kickoffThenWait.initiate({
              kickoff: {
                url: `/api/moderation-service/api/episode-reports/${PodcastEpisodeId}`,
                method: "POST",
                authMode: "required",
                body: {
                  EpisodeReportCreateInfo: {
                    Content: Content,
                    PodcastEpisodeReportTypeId: ReportTypeId,
                  },
                },
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
  }),
});

export const {
    useGetShowReportTypesQuery,
    useGetEpisodeReportTypesQuery,
    useReportShowMutation,
    useReportEpisodeMutation,
} = reportApi;
