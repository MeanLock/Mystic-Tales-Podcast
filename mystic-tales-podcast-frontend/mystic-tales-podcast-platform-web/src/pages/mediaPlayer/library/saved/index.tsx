import {
  useGetSavedEpisodesQuery,
  useSaveEpisodeMutation,
} from "@/core/services/episode/episode.service";
import type { EpisodeFromAPI, EpisodeUI } from "@/core/types/episode";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";
import { useEffect, useState } from "react";
import EpisodeCard from "./components/EpisodeCard";

const fileResolve: FileResolveConfig[] = [
  {
    type: "PodcastPublic",
    path: "[].MainImageFileKey",
    output: "[].ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "[].PodcastShow.MainImageFileKey",
    output: "[].PodcastShow.ImageUrl",
  },
];

const SavedPage = () => {
  const [savedEpisodes, setSavedEpisodes] = useState<EpisodeUI[]>([]);

  const {
    data: savedEpisodesDataRaw,
    isLoading: isLoadingSavedEpisodes,
    refetch: refetchSavedEpisodes,
  } = useGetSavedEpisodesQuery(undefined, {
    refetchOnMountOrArgChange: true,
    refetchOnFocus: true,
    refetchOnReconnect: true,
  });

  useEffect(() => {
    const resolveFile = async () => {
      if (isLoadingSavedEpisodes) return;
      const list = savedEpisodesDataRaw?.SavedEpisodes ?? [];
      if (!Array.isArray(list) || list.length === 0) {
        setSavedEpisodes([]);
        return;
      }

      const { resolvedData } = await resolveFiles<EpisodeFromAPI[]>(
        list,
        fileResolve
      );

      setSavedEpisodes(resolvedData as unknown as EpisodeUI[]);
    };
    resolveFile();
  }, [savedEpisodesDataRaw, isLoadingSavedEpisodes]);

  const [unsaveEpisode] = useSaveEpisodeMutation();
  const handleUnSaveEpisode = async (podcastEpisodeId: string) => {
    try {
      await unsaveEpisode({
        PodcastEpisodeId: podcastEpisodeId,
        IsSave: false,
      }).unwrap();
      await refetchSavedEpisodes();
    } catch (error) {
      console.error("Failed to unsave episode:", error);
    }
  };
  return (
    <div className="w-full h-full gap-10 flex flex-col">
      <h1 className="m-8 text-7xl font-bold font-poppins text-white mb-4">
        Saved Episodes
      </h1>
      {isLoadingSavedEpisodes ? (
        <div className="m-8 font-poppins text-[#D9D9D9]">
          Loading saved episodes...
        </div>
      ) : savedEpisodes.length === 0 ? (
        <div className="m-8 font-poppins text-[#D9D9D9]">
          No saved episodes yet.
        </div>
      ) : (
        <div className="grid mx-8 grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-10">
          {savedEpisodes.map((ep) => (
            <EpisodeCard
              key={ep.Id}
              episode={ep}
              handleUnSaveEpisode={handleUnSaveEpisode}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default SavedPage;
