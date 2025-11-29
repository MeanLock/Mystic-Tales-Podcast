// @ts-nocheck

import type { PodcastBuddyUI } from "@/core/types/booking";
import { FaEye, FaStar } from "react-icons/fa";
import { MdOutlinePeopleAlt, MdPeopleAlt } from "react-icons/md";
import { TbCoinFilled } from "react-icons/tb";
import { useNavigate } from "react-router-dom";

interface BuddyCardProps {
  buddy: PodcastBuddyUI;
  onViewDetails: (id: number) => void;
  onSelectBuddy: (buddy: PodcastBuddyUI) => void;
}

const BuddyCard = ({ buddy, onViewDetails, onSelectBuddy }: BuddyCardProps) => {

  return (
    <div onClick={() => onSelectBuddy(buddy)} className="relative cursor-pointer transition-all duration-500 ease-out hover:-translate-y-1 w-[300px] h-[150px] flex gap-2 p-2 backdrop-blur-md rounded-md">
      <div className="h-full flex items-center justify-center">
        <img
          src={buddy.PodcastBuddyProfile.ImageUrl}
          className="w-[134px] aspect-square shadow-2xl object-cover rounded-sm"
        />
      </div>
      <div className="text-white">
        <div className="flex flex-col gap-2">
          <p className="font-bold text-lg text-white line-clamp-1">
            {buddy.PodcastBuddyProfile.Name}
          </p>
          <div className="w-full flex items-center justify-start text-xs gap-1">
            <TbCoinFilled />
            <p className="font-semibold">
              {(
                buddy.PodcastBuddyProfile.PricePerBookingWord * 1000
              ).toLocaleString()}
            </p>
            <p>/1000 words</p>
          </div>

          <div className="w-full flex items-center justify-start text-xs gap-1">
            <MdOutlinePeopleAlt />
            <p className="font-semibold">
              {buddy.PodcastBuddyProfile.TotalFollow.toLocaleString()}
            </p>
            <p>Followers</p>
          </div>

          <div className="w-full flex items-center justify-start text-xs gap-1">
            <FaStar />
            <p className="font-semibold">
              {buddy.PodcastBuddyProfile.AverageRating}
            </p>
            <p>({buddy.PodcastBuddyProfile.RatingCount.toLocaleString()})</p>
          </div>
        </div>
      </div>

      <div
        onClick={() => onViewDetails(buddy.PodcastBuddyProfile.AccountId)}
        className="w-8 h-8 flex items-center justify-center rounded-full absolute right-2 bottom-2 text-white hover:text-mystic-green hover:bg-white/10"
      >
        <FaEye />
      </div>
    </div>
  );
};

export default BuddyCard;
