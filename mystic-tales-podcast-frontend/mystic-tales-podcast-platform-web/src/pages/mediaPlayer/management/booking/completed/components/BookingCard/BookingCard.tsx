import { IoPlaySkipForwardCircle } from "react-icons/io5";
import { useMemo } from "react";
import "./styles.css";
import type { CompletedBooking } from "@/core/types/booking";
import { useNavigate } from "react-router-dom";

const gradientThemes = [
  "bg-gradient-to-r from-[#ffe999] via-[#ff94f6] to-[#8ab9ff]",
  "bg-gradient-to-r from-[#a0c4ff] via-[#bdb2ff] to-[#ffc6ff]",
  "bg-gradient-to-r from-[#9bf6ff] via-[#fcbf49] to-[#ff006e]",
  "bg-gradient-to-r from-[#caffbf] via-[#fdffb6] to-[#ffd6a5]",
  "bg-gradient-to-r from-[#b5ead7] via-[#c7ceea] to-[#f1cbff]",
];

const BookingCard = ({ booking }: { booking: CompletedBooking }) => {
  const gradientClass = useMemo(
    () => gradientThemes[Math.floor(Math.random() * gradientThemes.length)],
    []
  );
  const navigate = useNavigate();
  return (
    <div className="relative aspect-video w-[260px] bg-white p-1 rounded-lg shadow-md transition-all duration-500 hover:shadow-xl hover:scale-105">
      <div className={`w-full h-full rounded-md ${gradientClass}`} />
      <div className="absolute inset-0 z-20 p-1 flex items-end justify-start">
        <div className="folderCard flex flex-col gap-2 p-2">
          <p className={`text-sm font-poppins font-semibold text-[#00645d]`}>
            Booking
          </p>
          <p className="text-[#252525] font-bold line-clamp-1 text-xs">
            {booking.Title}
          </p>
          <div className="flex-1 flex items-end w-full">
            <div className="flex w-full items-center justify-between">
              <div className="flex items-baseline font-poppins gap-1">
                <p className="font-bold text-2xl leading-none">
                  {booking.CompletedBookingTrackCount}
                </p>
                <p className="leading-none text-xs font-semibold text-[#252525]">
                  tracks
                </p>
              </div>
            </div>
            <div
              onClick={() =>
                navigate(
                  `/media-player/management/completed-bookings/${booking.Id}`
                )
              }
              className="w-[50px] aspect-square flex items-center justify-center rounded-full"
            >
              <IoPlaySkipForwardCircle
                className="transition-all duration-300 hover:scale-110 text-black hover:text-mystic-green"
                size={35}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
export default BookingCard;
