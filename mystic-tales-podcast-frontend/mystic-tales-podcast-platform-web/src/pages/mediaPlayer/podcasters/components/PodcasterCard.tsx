import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import type { PodcasterFromApi } from "@/core/types/podcaster";
import { MdPeopleAlt } from "react-icons/md";
import { useNavigate } from "react-router-dom";

interface PodcasterCardProps {
  podcaster: PodcasterFromApi;
}
const PodcasterCard = ({ podcaster }: PodcasterCardProps) => {
  const navigate = useNavigate();

  return (
    <div
      onClick={() =>
        navigate(`/media-player/podcasters/${podcaster.AccountId}`)
      }
      className=""
    >
      <div className="relative w-full flex flex-col items-center justify-center gap-5 p-5 rounded-md transition-all duration-700 hover:scale-105 ease-in-out cursor-pointer">
        {/* image */}
        <AutoResolveImage
          FileKey={podcaster.MainImageFileKey}
          type="AccountPublicSource"
          className="w-full aspect-square rounded-full object-cover"
        />
        {/* info */}
        <div className="w-full flex flex-col items-center gap-1">
          <div className="w-full flex items-center justify-center">
            <p className="text-lg font-semibold text-white font-poppins line-clamp-1">
              {podcaster.Name}
            </p>
          </div>

          <div className="w-full flex items-center justify-center gap-2">
            <MdPeopleAlt size={15} color="#d9d9d9" />
            <p className="font-light text-xs text-[#D9D9D9] font-poppins">
              {podcaster.TotalFollow.toLocaleString()}
            </p>
          </div>
        </div>

        {/* top */}
        {/* <div className="absolute rounded-bl-md top-0 right-0 w-12 h-16 flex items-center justify-center bg-mystic-green text-black font-bold shadow-2xl">
          <p>#{podcaster.Top}</p>
        </div> */}
      </div>
    </div>
  );
};

export default PodcasterCard;
