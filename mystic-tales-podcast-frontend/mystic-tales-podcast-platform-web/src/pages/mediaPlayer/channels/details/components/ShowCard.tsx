import type { ChannelUI } from "@/core/types/channel";
import type { ShowUI } from "@/core/types/show";
import { MdOutlinePodcasts } from "react-icons/md";

interface ShowCardProps {
  show: ShowUI;
}

const ShowCard = ({ show }: ShowCardProps) => {
  return (
    <div className="w-full flex flex-col gap-3 transition-all duration-500 ease-out hover:-translate-y-2 cursor-pointer">
      <div className="w-full relative">
        <img
          src={show.ImageUrl}
          className="aspect-square w-full object-cover rounded-md"
        />
      </div>

      <div className="w-full flex flex-col gap-1">
        <p className="text-white font-semibold line-clamp-2">{show.Name}</p>
        <p className="text-sm text-[#d9d9d9] font-md">{show.UploadFrequency}</p>
      </div>
    </div>
  );
};

export default ShowCard;
