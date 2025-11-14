// @ts-nocheck

import type { ChannelUI } from "@/core/types/channel";
import type { ShowUI } from "@/core/types/show";
import { MdOutlinePodcasts } from "react-icons/md";

interface ShowCardProps {
  show: ShowUI;
}

const ShowCard = ({ show }: ShowCardProps) => {
  return (
    <div className="w-full flex flex-col gap-3">
      <div className="w-full relative">
        <img
          src={show.ImageUrl}
          className="aspect-square w-full object-cover rounded-md"
        />
        <div className="flex items-center justify-center gap-1 absolute z-10 bg-black/50 px-2 py-1 rounded-md bottom-3 right-3">
          <MdOutlinePodcasts color="#fff" size={20} />
          <p className="text-sm text-white font-bold">
            {show.EpisodeCount} episodes
          </p>
        </div>
      </div>

      <div className="w-full flex items-center">
        <p className="text-white font-semibold line-clamp-1">{show.Name}</p>
      </div>
    </div>
  );
};

export default ShowCard;
