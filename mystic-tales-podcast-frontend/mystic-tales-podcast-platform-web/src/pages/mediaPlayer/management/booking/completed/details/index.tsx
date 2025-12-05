import Loading from "@/components/loading";
import { useGetCompletedBookingDetailQuery } from "@/core/services/booking/booking.service";
import type { CompletedBookingDetails } from "@/core/types/booking";
import { useNavigate, useParams } from "react-router-dom";
import AutoResolveImage from "./components/AutoResolveImage";
import BookingTrackRow from "./components/BookingTrackRow";
import { IoChevronBack } from "react-icons/io5";

const renderBookingInfo = (booking: CompletedBookingDetails) => {
  let infoString = "";
  const totalAudioLengthInSeconds = booking.LastestBookingPodcastTracks.reduce(
    (total, track) => total + track.AudioLength,
    0
  );
  const minutes = Math.floor(totalAudioLengthInSeconds / 60);
  const seconds = totalAudioLengthInSeconds % 60;
  infoString += `${booking.LastestBookingPodcastTracks.length} tracks • ${minutes}m ${seconds}s`;
  return infoString;
};

const CompletedBookingDetailsPage = () => {
  // STATES
  const { id } = useParams<{ id: string }>();

  // HOOKS
  const { data: bookingDetailsRaw, isLoading: isLoadingBookingDetails } =
    useGetCompletedBookingDetailQuery({ BookingId: Number(id) });
  const navigate = useNavigate();

  // RENDER
  if (isLoadingBookingDetails) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="text-[#d9d9d9] font-poppins font-bold">
          Loading completed booking details...
        </p>
      </div>
    );
  }

  if (!bookingDetailsRaw) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        <p className="text-red-500 font-poppins font-bold">
          Failed to load completed booking details.
        </p>
      </div>
    );
  }

  return (
    <div className="w-full flex flex-col">
      <div className="w-full">
        <div
          onClick={() => navigate(-1)}
          className="flex items-center gap-2 m-8 text-white cursor-pointer hover:underline font-poppins"
        >
          <IoChevronBack />
          <p>Back</p>
        </div>
      </div>

      {/* Booking Informations */}
      <div className="w-full relative p-8 bg-gradient-to-r from-[#667eea] to-[#c41404]">
        <div className="w-full relative h-[260px] flex gap-8 ">
          {/* Image Aesthetic */}
          <div className="w-[260px] h-[260px] rounded-lg shadow-lg overflow-hidden">
            <img
              src="https://i.pinimg.com/1200x/a7/4d/41/a74d41df10e0700557259776ef26beb1.jpg"
              className="w-full h-full object-cover"
              alt="Booking Image"
            />
          </div>
          {/* Booking Info */}
          <div className="h-[260px] flex-1 flex flex-col items-start justify-end gap-10 py-5">
            <p className="text-7xl pb-4 font-bold text-white line-clamp-1">
              {bookingDetailsRaw?.BookingList.Title}
            </p>
            <div className="w-full flex items-center gap-1">
              <AutoResolveImage
                FileKey={bookingDetailsRaw.BookingList.Account.MainImageFileKey}
              />
              <p className="font-poppins ml-2 font-bold text-white  text-sm leading-none">
                {bookingDetailsRaw?.BookingList.Account.FullName}
              </p>
              <p className="font-poppins font-light text-white text-sm leading-none">
                • {renderBookingInfo(bookingDetailsRaw!.BookingList)}
              </p>
            </div>
          </div>
        </div>
        {/* Player Management */}
        <div className="flex-1 flex items-center justify-start gap-5">
          <div></div>
        </div>
      </div>

      {/* Track List */}
      <div className="w-full flex flex-col gap-2 mt-10">
        {bookingDetailsRaw.BookingList.LastestBookingPodcastTracks.map(
          (track, index) => (
            <BookingTrackRow track={track} index={index} key={track.Id} />
          )
        )}
      </div>
    </div>
  );
};
export default CompletedBookingDetailsPage;
