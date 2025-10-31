import type { ChannelUI } from "@/core/types/channel";
import { MdOutlinePodcasts } from "react-icons/md";

interface ChannelCardProps {
  channel: ChannelUI;
}

const ChannelCard = ({ channel }: ChannelCardProps) => {
  return (
    <div className="w-full flex flex-col gap-3">
      <div className="w-full relative">
        <img
          src={channel.ImageUrl}
          className="aspect-square w-full object-cover rounded-md"
        />
        <div className="flex items-center justify-center gap-1 absolute z-10 bg-black/50 px-2 py-1 rounded-md bottom-3 right-3">
          <MdOutlinePodcasts color="#fff" size={20} />
          <p className="text-sm text-white font-bold">
            {channel.ShowCount} shows
          </p>
        </div>
      </div>

      <div className="w-full flex items-center">
        <p className="text-white font-semibold line-clamp-1">{channel.Name}</p>
      </div>
    </div>
  );
};

export default ChannelCard;
