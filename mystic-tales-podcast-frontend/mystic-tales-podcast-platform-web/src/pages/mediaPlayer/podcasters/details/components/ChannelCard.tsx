import type { ChannelUI } from "@/core/types/channel";
import { FaPodcast, FaStar } from "react-icons/fa";
import { MdOutlinePodcasts } from "react-icons/md";

interface ChannelCardProps {
  channel: ChannelUI;
}

const ChannelCard = ({ channel }: ChannelCardProps) => {
  return (
    <div
      className="group p-3 relative w-full flex flex-col gap-2 rounded-2xl overflow-hidden    
                 shadow-[0_4px_30px_rgba(0,0,0,0.1)]
                 transition-all duration-500 ease-out hover:scale-105 hover:border-white/60 
                 hover:shadow-[0_0_8px_rgba(255,255,255,1)] cursor-pointer"
    >
      {/* inner glass border nhẹ */}
      {/* <div
        className="absolute inset-0 rounded-2xl 
                   border border-white/10 
                   [box-shadow:inset_0_0_15px_rgba(255,255,255,0.05)] 
                   pointer-events-none"
      ></div> */}

      <div className="relative w-full flex items-start justify-center p-2 z-10">
        <img
          src={channel.ImageUrl}
          className="aspect-square w-full object-cover rounded-md mx-auto shadow-[5px_5px_15px_#0000005c]"
        />
      </div>

      <div className="w-full flex flex-col items-start justify-center gap-1 p-2 z-10">
        {/* <p className="text-xs font-semibold text-gray-300 line-clamp-1">
          #Lauv_You
        </p> */}
        <div className="flex items-center gap-1 text-[12px] font-semibold text-[#d9d9d9]">
          <FaPodcast />
          <p className="line-clamp-1">12 shows</p>
        </div>
        <p className="text-white text-[15px] font-semibold line-clamp-1">
          {channel.Name}
        </p>

        <p className="text-[10px] font-medium text-[#d9d9d9]">
          PSYCHOLOGY • HUMAN SOCIETY
        </p>

        <div className="italic flex items-center gap-1 text-[10px] font-md hover:text-mystic-green text-[#d9d9d9]">
          #Show_Your_Lauv
        </div>
      </div>
    </div>
  );
};

export default ChannelCard;
