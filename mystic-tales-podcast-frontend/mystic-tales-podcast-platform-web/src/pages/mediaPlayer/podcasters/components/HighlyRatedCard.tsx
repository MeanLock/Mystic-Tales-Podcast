// @ts-nocheck
import { MdPeopleAlt } from "react-icons/md";
import { FaStar } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import type { PodcasterFromApi } from "@/core/types/podcaster";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

interface PodcasterCardProps {
  podcaster: PodcasterFromApi;
}
const HighlyRatedCard = ({ podcaster }: PodcasterCardProps) => {
  const navigate = useNavigate();
  return (
    <div
      onClick={() =>
        navigate(`/media-player/podcasters/${podcaster.AccountId}`)
      }
      className="relative w-full flex flex-col items-center justify-center gap-5 p-5 rounded-md transition-all duration-300 hover:scale-105 ease-out cursor-pointer"
    >
      {/* image */}
      <AutoResolveImage
        FileKey={podcaster.MainImageFileKey}
        type="AccountPublicSource"
        className="w-full aspect-square rounded-full object-cover"
      />
      {/* info */}
      <div className="w-full flex flex-col items-center gap-1">
        <p className="text-lg font-semibold text-white font-poppins">
          {podcaster.Name}
        </p>
        <div className="w-full flex items-center justify-center gap-2">
          <p className="font-bold text-xs text-[#D9D9D9] font-poppins">
            {podcaster.AverageRating.toFixed(2)}
          </p>
          <FaStar size={15} color="#d9d9d9" />
        </div>
      </div>
    </div>
  );
};

export default HighlyRatedCard;
