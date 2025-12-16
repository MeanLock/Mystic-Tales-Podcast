// @ts-nocheck

import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import type { TopPodcasters } from "@/core/types/feed";
import type { PodcasterUI } from "@/core/types/podcaster";

import { MdPeopleAlt } from "react-icons/md";
import { useNavigate } from "react-router-dom";

interface PodcasterCardProps {
  podcaster: TopPodcasters["PodcasterList"][number];
}
const PodcasterCard = ({ podcaster }: PodcasterCardProps) => {
  const navigate = useNavigate();

  return (
    <div
      onClick={() => navigate(`/media-player/podcasters/${podcaster.Id}`)}
      className=""
    >
      <div className="relative w-full flex flex-col items-center justify-center gap-5 p-5 rounded-md transition-all duration-700 hover:scale-105 ease-in-out cursor-pointer">
        {/* image */}
        {/* <img
          src={podcaster.ImageUrl}
          alt={podcaster.FullName}
          className="w-full aspect-square rounded-full object-cover"
        /> */}
        <AutoResolveImage
          FileKey={podcaster.MainImageFileKey}
          type="PodcastPublicSource"
          Name={podcaster.FullName || "podcaster-image"}
          imgClassName="w-full aspect-square rounded-full object-cover"
        />

        {/* info */}
        <div className="w-full flex flex-col items-center gap-1">
          <div className="w-full flex items-center justify-center">
            <p className="text-lg line-clamp-1 text-center font-semibold text-white font-poppins">
              {podcaster.FullName}
            </p>
          </div>

          <div className="w-full flex items-center justify-center gap-2">
            <p className="font-light text-xs text-[#D9D9D9] font-poppins">
              {podcaster.Email}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PodcasterCard;
