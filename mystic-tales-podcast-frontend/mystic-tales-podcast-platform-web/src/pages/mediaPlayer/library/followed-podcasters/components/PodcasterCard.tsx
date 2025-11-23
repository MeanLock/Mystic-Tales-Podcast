// @ts-nocheck

import type { PodcasterUI } from "@/core/types/podcaster";
import type { TopPodcasterUI } from "..";
import { MdPeopleAlt } from "react-icons/md";
import { useNavigate } from "react-router-dom";

interface PodcasterCardProps {
  podcaster: TopPodcasterUI;
  onViewDetails: () => void;
}
const PodcasterCard = ({ podcaster, onViewDetails }: PodcasterCardProps) => {
  const navigate = useNavigate();

  return (
    <div onClick={onViewDetails} className="">
      <div className="relative w-full flex flex-col items-center justify-center gap-5 p-5 rounded-md transition-all duration-700 hover:scale-105 ease-in-out cursor-pointer">
        {/* image */}
        <img
          src={podcaster.ImageUrl}
          alt={podcaster.Name}
          className="w-full aspect-square rounded-full object-cover"
        />

        {/* info */}
        <div className="w-full flex flex-col items-center gap-1">
          <div className="w-full flex items-center justify-center">
            <p className="text-lg font-semibold text-white font-poppins">
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
      </div>
    </div>
  );
};

export default PodcasterCard;
