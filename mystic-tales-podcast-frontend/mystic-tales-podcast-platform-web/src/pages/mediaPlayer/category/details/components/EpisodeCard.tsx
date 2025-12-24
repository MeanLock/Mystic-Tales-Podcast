import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import type { EpisodeFromAPI } from "@/core/types/episode";
import { FaHeadphones } from "react-icons/fa";
import { useNavigate } from "react-router-dom";

const EpisodeCard = ({ episode }: { episode: EpisodeFromAPI }) => {
  const formatNumber = (num: number): string => {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1).replace(/\.0$/, "") + "M";
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1).replace(/\.0$/, "") + "k";
    }
    return num.toString();
  };

  const formatDuration = (seconds: number): string => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);

    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  };

  const navigate = useNavigate();

  return (
    <div
      onClick={() => navigate(`/media-player/episodes/${episode.Id}`)}
      className="w-full aspect-3/4 bg-white rounded-md overflow-hidden relative cursor-pointer transition-all duration-500 ease-out hover:-translate-y-1.5"
    >
      <div className="absolute inset-0 flex items-center justify-center">
        <AutoResolveImage
          FileKey={episode.MainImageFileKey}
          type="PodcastPublicSource"
          imgClassName="w-full aspect-[3/4] rounded-md"
        />
      </div>

      <div className="absolute z-20 p-3 inset-0 bg-linear-to-t from-black to-transparent flex flex-col justify-between gap-1">
        {/* Stats */}
        <div className="w-full flex items-center justify-between">
          <div className="w-fit px-2 py-1 shadow-md rounded-full bg-white/40 text-[#252525] flex items-center gap-2">
            <FaHeadphones />
            <p className="font-poppins font-bold text-xs line-clamp-1">
              {formatNumber(episode.ListenCount)}
            </p>
          </div>
          {episode.AudioLength && (
            <div className="w-fit px-2 py-1 shadow-md rounded-full bg-white/40 text-[#252525]">
              <p className="font-poppins font-bold text-xs">
                {formatDuration(episode.AudioLength)}
              </p>
            </div>
          )}
        </div>

        <div className="w-full flex flex-col">
          <p className="font-bold text-white text-lg line-clamp-2">
            {episode.Name}
          </p>
          {episode.PodcastShow && (
            <p className="text-sm text-gray-300 line-clamp-1">
              {episode.PodcastShow.Name}
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default EpisodeCard;
