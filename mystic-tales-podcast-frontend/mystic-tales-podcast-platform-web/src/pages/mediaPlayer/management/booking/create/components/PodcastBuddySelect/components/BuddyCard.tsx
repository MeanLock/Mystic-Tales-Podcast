import type { PodcastBuddyFromAPI } from "@/core/types/booking";
import { FaEye, FaStar } from "react-icons/fa";
import { MdFactCheck } from "react-icons/md";
import { TbCoinFilled } from "react-icons/tb";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

interface BuddyCardProps {
  buddy: PodcastBuddyFromAPI;
  onViewDetails: (id: number) => void;
  onSelectBuddy: (buddy: PodcastBuddyFromAPI) => void;
}

const BuddyCard = ({ buddy, onViewDetails, onSelectBuddy }: BuddyCardProps) => {
  return (
    <div
      key={buddy.Id}
      onClick={() => onSelectBuddy(buddy)}
      className="relative w-87.5 h-50 flex items-center justify-center bg-white rounded-md shadow-sm"
    >
      <div className="gradient-over-lay rounded-md w-full h-full bg-linear-to-r from-blue-300 to-red-300"></div>
      <div className="absolute inset-0 z-10 rounded-md">
        <div className="absolute top-4.5 left-3 z-20">
          <AutoResolveImage
            FileKey={buddy.MainImageFileKey}
            type="AccountPublicSource"
            className="w-15 aspect-square shadow-md object-cover rounded-full"
          />
        </div>
        <div className="absolute inset-x-0 bottom-0 h-32.5 w-full rounded-b-md bg-white flex flex-col p-3">
          <p className="font-poppins font-semibold text-sm text-[#252525]">
            {buddy.FullName}
          </p>
          <p className="text-xs font-medium text-[#616161]">{buddy.Email}</p>
          <div className="flex-1 grid grid-cols-5 py-2">
            <div className="col-span-1 flex flex-col items-center justify-center border-r-[#D9D9D9] border-r">
              <div className="w-full flex items-center justify-center gap-2 text-sm">
                <FaStar />
                <p className="font-semibold font-poppins">
                  {buddy.AverageRating.toFixed(1)}
                </p>
              </div>
              <p className="text-xs text-[#616161] font-poppins">ratings</p>
            </div>
            <div className="col-span-1 flex flex-col items-center justify-center border-r-[#D9D9D9] border-r">
              <div className="w-full flex items-center justify-center gap-2 text-sm">
                <MdFactCheck />
                <p className="font-semibold font-poppins">
                  {buddy.TotalBookingCompleted.toLocaleString()}
                </p>
              </div>
              <p className="text-xs text-[#616161] font-poppins">bookings</p>
            </div>
            <div className="col-span-2 flex flex-col items-center justify-center border-r-[#D9D9D9] border-r]">
              <div className="w-full flex items-center justify-center gap-2 text-sm">
                <TbCoinFilled />
                <p className="font-semibold font-poppins">
                  {(buddy.PriceBookingPerWord * 100).toLocaleString()}
                </p>
              </div>
              <p className="text-xs text-[#616161] font-poppins">
                /per 100 words
              </p>
            </div>
            <div className="col-span-1  flex flex-col items-center justify-center">
              <div
                onClick={() => onViewDetails(buddy.Id)}
                className="cursor-pointer bg-gray-300 w-8 h-8 flex items-center justify-center rounded-full text-white transition-colors duration-200 hover:text-black hover:bg-mystic-green"
              >
                <FaEye />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default BuddyCard;
