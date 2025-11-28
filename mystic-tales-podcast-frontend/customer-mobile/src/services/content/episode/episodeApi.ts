// services/content/episode/episodeApi.ts
import { baseApi } from "../../baseApi";
import type { EpisodeFromApi, EpisodeWithImageUrl } from "@/src/types/episode";

/**
 * Helper: map EpisodeFromApi -> EpisodeWithImageUrl (chưa có ImageUrl)
 */
function baseMap(ep: EpisodeFromApi, imageUrl: string): EpisodeWithImageUrl {
  return {
    Id: ep.Id,
    Title: ep.Title,
    Description: ep.Description,
    ExplicitContent: ep.ExplicitContent,
    ReleaseDate: ep.ReleaseDate,
    IsReleased: ep.IsReleased,
    ImageUrl: imageUrl, // <- thêm
    AudioFileKey: ep.AudioFileKey,
    AudioFileSize: ep.AudioFileSize,
    AudioLength: ep.AudioLength,
    AudioFingerprint: ep.AudioFingerprint,
    PodcastEpisodeSubscriptionType: ep.PodcastEpisodeSubscriptionType as any,
    PodcastShowId: ep.PodcastShowId,
    SeasonNumber: ep.SeasonNumber,
    TotalSave: ep.TotalSave,
    ListenCount: ep.ListenCount,
    IsAudioPublishable: ep.IsAudioPublishable,
    TakenDownReason: ep.TakenDownReason,
    DeletedAt: ep.DeletedAt,
    CreatedAt: ep.CreatedAt,
    UpdatedAt: ep.UpdatedAt,
  };
}

export const episodeApi = baseApi.injectEndpoints({
  endpoints: (b) => ({
    /**
     * Trả danh sách EpisodeWithImageUrl[]
     * - Gọi /episodes
     * - Với mỗi MainImageFileKey -> gọi get-file-url để lấy URL hình
     */
    getEpisodesWithImageUrl: b.query<EpisodeWithImageUrl[], void>({
      async queryFn(_arg, _api, _extra, fetchWithBQ) {
        // 1) Lấy danh sách episode
        const epRes = await fetchWithBQ({
          url: `/api/podcast-service/api/episodes`,
          method: "GET",
        });
        if (epRes.error) return { error: epRes.error as any };

        const episodes = (epRes.data as EpisodeFromApi[]) ?? [];

        // 2) Resolve từng MainImageFileKey -> url
        // (chạy song song; có thể thêm giới hạn concurrency nếu cần)
        const mapped = await Promise.all(
          episodes.map(async (ep) => {
            const key = ep.MainImageFileKey;
            let imageUrl = "";

            if (key) {
              const r = await fetchWithBQ({
                url: `/api/user-service/api/misc/file-source/get-file-url/${encodeURIComponent(
                  key
                )}`,
                method: "GET",
              });
              if (!r.error) {
                const data: any = (r.data as any)?.data ?? r.data;
                imageUrl = String(
                  data?.FileUrl ?? data?.Url ?? data?.URL ?? ""
                );
              }
            }
            return baseMap(ep, imageUrl);
          })
        );

        return { data: mapped };
      },

      // Cache 5 phút (tuỳ chỉnh)
      keepUnusedDataFor: 300,
      // Nếu muốn invalidation theo tag, thêm "Episodes" vào baseApi.tagTypes rồi uncomment:
      providesTags: (res) =>
        res
          ? [
              "Episodes",
              ...res.map((e) => ({ type: "Episodes" as const, id: e.Id })),
            ]
          : ["Episodes"],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetEpisodesWithImageUrlQuery,
  useLazyGetEpisodesWithImageUrlQuery,
} = episodeApi;
