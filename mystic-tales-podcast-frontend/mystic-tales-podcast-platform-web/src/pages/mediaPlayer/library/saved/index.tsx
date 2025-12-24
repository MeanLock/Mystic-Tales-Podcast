import {
  useGetSavedEpisodesQuery,
  useSaveEpisodeMutation,
} from "@/core/services/episode/episode.service";
import EpisodeCard from "./components/EpisodeCard";

const SavedPage = () => {

  const {
    data: savedEpisodes,
    isLoading: isLoadingSavedEpisodes,
    refetch: refetchSavedEpisodes,
  } = useGetSavedEpisodesQuery(undefined, {
    refetchOnMountOrArgChange: true,
    refetchOnFocus: true,
    refetchOnReconnect: true,
  });

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
      ) : !savedEpisodes || savedEpisodes.SavedEpisodes.length === 0  ? (
        <div className="m-8 font-poppins text-[#D9D9D9]">
          No saved episodes yet.
        </div>
      ) : (
        <div className="grid mx-8 grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-10">
          {savedEpisodes.SavedEpisodes.map((ep) => (
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
