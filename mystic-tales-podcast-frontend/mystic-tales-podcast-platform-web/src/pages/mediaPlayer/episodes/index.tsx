import type { EpisodeFromAPI } from "@/core/types/episode";
import type { RootState } from "@/redux/store";
import { useEffect, useState } from "react";
import { IoArrowBack } from "react-icons/io5";
import { useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import EpisodeCard from "./components/EpisodeCard";


type EpisodeListBySeason = {
  SeasonNumber: number;
  Episodes: EpisodeFromAPI[];
};

// comparator: latest EpisodeOrder first, tie-breaker by latest CreatedAt
function compareEpisodes(a: EpisodeFromAPI, b: EpisodeFromAPI) {
  // EpisodeOrder desc
  if (a.EpisodeOrder !== b.EpisodeOrder) {
    return b.EpisodeOrder - a.EpisodeOrder;
  }

  // CreatedAt desc (newest first)
  const aTime = new Date(a.CreatedAt).getTime();
  const bTime = new Date(b.CreatedAt).getTime();

  return bTime - aTime;
}

const EpisodeListPage = () => {
  const episodeData = useSelector((state: RootState) => state.seeMoreEpisode);
  const [episodesBySeason, setEpisodesBySeason] = useState<
    EpisodeListBySeason[]
  >([]);
  const navigate = useNavigate();

  useEffect(() => {
    if (!episodeData || !episodeData.episodes || episodeData.title === "") {
      navigate("/media-player/discovery");
      return;
    }

    // Group episodes by season
    const map = new Map<number, EpisodeFromAPI[]>();

    for (const ep of episodeData.episodes) {
      const season = ep.SeasonNumber ?? 0;
      if (!map.has(season)) {
        map.set(season, []);
      }
      map.get(season)!.push(ep);
    }

    // Convert Map -> array, sort season, sort episodes trong từng season
    const result: EpisodeListBySeason[] = Array.from(map.entries())
      .sort((a, b) => b[0] - a[0]) // SeasonNumber asc
      .map(([SeasonNumber, episodesInSeason]) => ({
        SeasonNumber,
        Episodes: [...episodesInSeason].sort(compareEpisodes),
      }));

    setEpisodesBySeason(result);
  }, [episodeData, navigate]);

  if (!episodeData || episodesBySeason.length === 0) {
    return (
      <div className="w-full h-full flex flex-col">
        <div
          onClick={() => navigate(-1)}
          className="w-full p-8 flex items-center gap-2 text-white hover:underline cursor-pointer font-poppins"
        >
          <IoArrowBack />
          <p>Back</p>
        </div>
        <div className="flex-1 flex items-center justify-center">
          <p className="text-red-500 font-poppins font-bold">
            No episodes to display.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full h-full flex flex-col">
      <div
        onClick={() => navigate(-1)}
        className="w-full p-8 flex items-center gap-2 text-white hover:underline cursor-pointer font-poppins"
      >
        <IoArrowBack />
        <p>Back</p>
      </div>
      <p className="text-white m-8 font-poppins text-5xl mb-10 pb-4 font-bold">
        {episodeData.title}
      </p>
      <div className="w-full flex flex-col gap-10 px-8 pb-10">
        {episodesBySeason.map((season) => (
          <section key={season.SeasonNumber} className="flex flex-col gap-4">
            <p className="text-white/80 font-poppins text-base font-semibold">
              Season {season.SeasonNumber}
            </p>

            <div className="flex flex-col gap-3">
              {season.Episodes.map((episode) => (
                <EpisodeCard key={episode.Id} episode={episode} />
              ))}
            </div>
          </section>
        ))}
      </div>
    </div>
  );
};

export default EpisodeListPage;
