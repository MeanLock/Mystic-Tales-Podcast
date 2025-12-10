// @ts-nocheck

import type {
  BookingDetailsUI,
  BookingFromAPI,
  BookingStatusType,
} from "@/core/types/booking";
import { IoPlay } from "react-icons/io5";
import { MdRemoveRedEye } from "react-icons/md";
import { useNavigate } from "react-router-dom";

const RenderStatus = (status: BookingStatusType) => {
  switch (status.Id) {
    case 1: {
      return (
        <div className="bg-[#C5A7CD]/40 px-2 py-1 rounded-full text-[#f5ddfc] flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 2: {
      return (
        <div className="bg-[#74B8CE]/40 px-2 py-1 rounded-full text-[#90e0fb] flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 3: {
      return (
        <div className="bg-[#F86247]/20 px-2 py-1 rounded-full text-[#fea595] flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 4: {
      return (
        <div className="bg-[#d14136]/30 px-2 py-1 rounded-full text-[#ff7d73] flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 5: {
      return (
        <div className="bg-[#d1b236]/20 px-2 py-1 rounded-full text-[#ffe371] flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 6: {
      return (
        <div className="bg-indigo-300/20 px-2 py-1 rounded-full text-indigo-100 flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 7: {
      return (
        <div className="bg-[#8d45db]/20 px-2 py-1 rounded-full text-[#d5affd] flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 8: {
      return (
        <div className="bg-green-500/20 px-2 py-1 rounded-full text-green-300 flex items-center justify-center text-sm line-clamp-1">
          <p>{status.Name}</p>
        </div>
      );
    }
    case 9: {
      return (
        <div className="bg-[#d14136]/30 px-2 py-1 rounded-full text-[#f7a6a0] flex items-center justify-center text-sm line-clamp-1">
          <p>Cancel Request (By You)...</p>
        </div>
      );
    }
    case 10: {
      return (
        <div className="bg-[#d14136]/30 px-2 py-1 rounded-full text-[#f7a6a0] flex items-center justify-center text-sm line-clamp-1">
          <p>Cancel Request (By Buddy)...</p>
        </div>
      );
    }
    case 11: {
      return (
        <div className="bg-[#d14136]/30 px-2 py-1 rounded-full text-[#f7a6a0] flex items-center justify-center text-sm line-clamp-1">
          <p>Cancelled Automatically</p>
        </div>
      );
    }
    case 12: {
      return (
        <div className="bg-[#d14136]/30 px-2 py-1 rounded-full text-[#f7a6a0] flex items-center justify-center text-sm line-clamp-1">
          <p>Cancelled Manually</p>
        </div>
      );
    }
  }
};

const BookingCard = ({ booking }: { booking: BookingFromAPI }) => {
  const navigate = useNavigate();

  return (
    <div
      key={booking.Id}
      className="grid text-white h-20 items-center grid-cols-12 px-3 py-3 font-semibold bg-white/10 cursor-pointer hover:bg-white/30 backdrop-blur-md shadow-[2px_2px_10px_#0000005c] rounded-xl transition-all duration-500 hover:-translate-y-1 ease-out"
    >
      <div className="col-span-1 flex items-center justify-start pl-3">
        <p className="font-light">{booking.Id}</p>
      </div>
      <div className="col-span-3 overflow-ellipsis">
        <p className="font-light line-clamp-1">{booking.Title}</p>
      </div>
      <div className="col-span-2 overflow-ellipsis">
        <p className="font-light line-clamp-1">
          {booking.PodcastBuddy
            ? booking.PodcastBuddy.FullName
            : "Unknown Podcaster"}
        </p>
      </div>
      <div className="col-span-1 overflow-ellipsis">
        <p className="font-bold line-clamp-1">
          {booking.Price ? booking.Price.toLocaleString("vn") : 0}
        </p>
      </div>
      <div className="col-span-2 text-center">
        <p className="font-light line-clamp-1">
          {booking.Deadline
            ? new Date(booking.Deadline).toLocaleDateString()
            : "Not Yet"}
        </p>
      </div>
      <div className="col-span-2 text-center">
        {RenderStatus(booking.CurrentStatus)}
      </div>
      <div className="col-span-1 flex items-center justify-center">
        <div
          onClick={() =>
            navigate(`/media-player/management/bookings/${booking.Id}`)
          }
          className="p-2 rounded-full flex items-center justify-center text-lg bg-transparent text-white transition-all duration-500 hover:bg-mystic-green hover:text-black hover:-translate-y-1"
        >
          <MdRemoveRedEye />
        </div>
      </div>
    </div>
  );
};

export default BookingCard;
