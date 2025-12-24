import type { ChannelFromAPI } from "@/core/types/channel";
import { FaHeart } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

const ChannelCard = ({
  channel,
}: {
  channel: ChannelFromAPI;
  isLoadingImage?: boolean;
}) => {
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
      onClick={() => navigate(`/media-player/channels/${channel.Id}`)}
      className="w-full aspect-square rounded-md shadow-2xl overflow-hidden relative cursor-pointer transition-all duration-500 ease-out hover:scale-105 hover:-translate-y-1.5"
    >
      <div className="absolute inset-0 flex items-center justify-center">
      <AutoResolveImage
          FileKey={channel.MainImageFileKey}
          type="PodcastPublicSource"
          className="w-full h-full object-cover"
        />
      </div>
      <div className="absolute inset-0 z-10 flex items-center justify-center bg-black/30"></div>
      <div className="w-full h-full z-20 flex flex-col justify-between relative p-3">
        <div className="w-fit px-2 py-1 shadow-md rounded-full bg-white/40 text-white flex items-center gap-2">
          <FaHeart />
          <p className="font-poppins font-bold text-xs line-clamp-1">
            {formatNumber(channel.TotalFavorite)}
          </p>
        </div>

        <div className="w-full flex flex-col gap-1">
          <p className="uppercase text-white font-bold text-sm line-clamp-1">
            {channel.Name}
          </p>
          <p className="text-[#D9D9D9] font-light">
            {channel.Podcaster.FullName}
          </p>
        </div>
      </div>
    </div>
  );
};

export default ChannelCard;
