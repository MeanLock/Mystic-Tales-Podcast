import { usePlayer } from "@/core/services/player/usePlayer";
import type { CompletedBookingTrack } from "@/core/types/booking";
import { IoPause, IoPlay } from "react-icons/io5";


const formatAudioLength = (lengthInSeconds: number) => {
  const hour = Math.floor(lengthInSeconds / 3600);
  if (hour > 0) {
    const minutes = Math.floor((lengthInSeconds % 3600) / 60);
    const seconds = lengthInSeconds % 60;
    return `${hour}:${minutes}:${seconds}`;
  }
  const minutes = Math.floor(lengthInSeconds / 60);
  const seconds = lengthInSeconds % 60;
  let finalMinutes = minutes < 10 ? `0${minutes}` : `${minutes}`;
  let finalSeconds = seconds < 10 ? `0${seconds}` : `${seconds}`;
  return `${finalMinutes}:${finalSeconds}`;
};

const BookingTrackRow = ({
  track,
  index,
}: {
  track: CompletedBookingTrack;
  index: number;
}) => {
  const { playBookingTrack, pause, state: uiState } = usePlayer();
  const handlePlayAudio = () => {
    playBookingTrack({
      bookingId: track.BookingId,
      bookingTrackId: track.Id,
    });
  };

  return (
    <div className="w-full h-[40px] grid grid-cols-12">
      <div className="col-span-1 flex items-center justify-center text-gray-400">
        {index + 1}
      </div>
      <div className="col-span-1 flex items-center justify-center text-gray-400">
        <img
          src="https://i.pinimg.com/736x/c2/a3/53/c2a3538e849197b336b9226722f9a63a.jpg"
          className="w-[36px] h-[36px] rounded-md object-cover shadow-sm"
        />
      </div>
      <div className="col-span-5 flex flex-col items-start justify-center text-gray-400">
        <p className="text-sm line-clamp-1">Track: {index + 1}</p>
        <p className="text-xs line-clamp-1">Booking: #{track.BookingId}</p>
      </div>
      <div className="col-span-4 flex items-center justify-center text-gray-400">
        <p>{formatAudioLength(track.AudioLength)}</p>
      </div>
      <div className="col-span-1 flex items-center justify-center text-gray-400">
        {/* <div className="p-2 rounded-full bg-white/20 hover:bg-white/60 transition-all duration-500 hover:scale-105 flex items-center justify-center">
          <IoPlay size={17} />
        </div> */}
        {uiState.isPlaying && uiState.currentAudio &&
        uiState.currentAudio.id === track.Id ? (
          <div
            onClick={() => pause()}
            className="p-2 rounded-full bg-white/20 hover:bg-white/60 transition-all duration-500 hover:scale-105 flex items-center justify-center"
          >
            <IoPause size={17} />
          </div>
        ) : (
          <div
            onClick={() => handlePlayAudio()}
            className="p-2 rounded-full bg-white/20 hover:bg-white/60 transition-all duration-500 hover:scale-105 flex items-center justify-center"
          >
            <IoPlay size={17} />
          </div>
        )}
      </div>
    </div>
  );
};
export default BookingTrackRow;
