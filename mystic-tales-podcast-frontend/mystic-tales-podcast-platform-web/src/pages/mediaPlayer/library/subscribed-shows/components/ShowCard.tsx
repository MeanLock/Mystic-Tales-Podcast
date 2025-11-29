import type { ShowUI } from "@/core/types/show";
import { FaHeadphones, FaHeart } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import { Skeleton } from "@/components/ui/skeleton";

const ShowCard = ({
  show,
  isLoadingImage,
}: {
  show: ShowUI;
  isLoadingImage: boolean;
}) => {
  const renderDescription = (show: ShowUI) => {
    const year = new Date(show.ReleaseDate).getFullYear();
    const category = show.PodcastCategory?.Name || "Unknown";
    const subcategory = show.PodcastSubCategory?.Name || "Unknown";
    const episodeCount = show.EpisodeCount.toLocaleString();

    return `${year} • ${category} • ${subcategory} • ${episodeCount} Episodes`;
  };

  const formatNumber = (num: number): string => {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1).replace(/\.0$/, "") + "M";
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1).replace(/\.0$/, "") + "k";
    }
    return num.toString();
  };

  const navigate = useNavigate();

  return (
    <div
      onClick={() => navigate(`/media-player/shows/${show.Id}`)}
      className="w-full aspect-video bg-white rounded-md overflow-hidden relative cursor-pointer transition-all duration-500 ease-out hover:-translate-y-[6px]"
    >
      <div className="absolute inset-0 flex items-center justify-center">
        {isLoadingImage ? (
          <Skeleton className="w-full h-full" />
        ) : (
          <img
            src={show.ImageUrl}
            alt={show.Name}
            className="w-full aspect-video object-cover"
          />
        )}
      </div>

      <div className="absolute z-20 p-3 inset-0 bg-gradient-to-t from-black to-transparent flex flex-col justify-between gap-1">
        {/* ListenCount */}
        <div className="w-fit px-2 py-1 shadow-md rounded-full bg-white/40 text-[#252525] flex items-center gap-2">
          <FaHeadphones />
          <p className="font-poppins font-bold text-xs line-clamp-1">
            {formatNumber(show.ListenCount)}
          </p>
        </div>
        <div className="w-full flex flex-col">
          <p className="font-bold text-white text-lg line-clamp-1">
            {show.Name}
          </p>
          <p className="text-sm text-gray-300 line-clamp-1">
            {renderDescription(show)}
          </p>
        </div>
      </div>
    </div>
  );
};

export default ShowCard;
