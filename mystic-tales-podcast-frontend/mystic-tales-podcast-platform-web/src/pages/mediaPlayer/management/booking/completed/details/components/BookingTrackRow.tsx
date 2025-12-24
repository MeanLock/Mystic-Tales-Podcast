import { usePlayer } from "@/core/services/player/usePlayer";
import type { CompletedBookingTrack } from "@/core/types/booking";
import { IoPause, IoPlay } from "react-icons/io5";
import { useCallback, useMemo } from "react";
import { debouncePromise } from "@/core/utils/debouncePromise";
import ActivityIndicator from "@/components/loader/ActivityIndicator";

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
  const { playBookingTrack, pause, play, state: uiState } = usePlayer();

  const playNewBookingTrack = useCallback(
    async (bookingId: number, bookingTrackId: string) => {
      await playBookingTrack({
        bookingId,
        bookingTrackId,
      });
    },
    [playBookingTrack]
  );

  const debouncedPlayNew = useMemo(
    () => debouncePromise(playNewBookingTrack, 1000),
    [playNewBookingTrack]
  );

  const handlePlayAudio = useCallback(() => {
    if (uiState.currentAudio?.id === track.Id) {
      if (uiState.isPlaying) {
        pause();
      } else {
        play();
      }
    } else {
      debouncedPlayNew(track.BookingId, track.Id);
    }
  }, [uiState, track.BookingId, track.Id, pause, play, debouncedPlayNew]);

  return (
    <div className="w-full h-10 grid grid-cols-12">
      <div className="col-span-1 flex items-center justify-center text-gray-400">
        {index + 1}
      </div>
      <div className="col-span-1 flex items-center justify-center text-gray-400">
        <img
          src="https://i.pinimg.com/736x/c2/a3/53/c2a3538e849197b336b9226722f9a63a.jpg"
          className="w-9 h-9 rounded-md object-cover shadow-sm"
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
        {uiState.isLoadingSession && uiState.loadingAudioId === track.Id ? (
          <div className="p-2 rounded-full bg-white/20 flex items-center justify-center">
            <ActivityIndicator size={17} color="white" />
          </div>
        ) : uiState.isPlaying && uiState.currentAudio?.id === track.Id ? (
          <div
            onClick={() => pause()}
            className="p-2 rounded-full bg-white/20 hover:bg-white/60 transition-all duration-500 hover:scale-105 flex items-center justify-center cursor-pointer"
          >
            <IoPause size={17} />
          </div>
        ) : (
          <div
            onClick={handlePlayAudio}
            className="p-2 rounded-full bg-white/20 hover:bg-white/60 transition-all duration-500 hover:scale-105 flex items-center justify-center cursor-pointer"
          >
            <IoPlay size={17} />
          </div>
        )}
      </div>
    </div>
  );
};
export default BookingTrackRow;
