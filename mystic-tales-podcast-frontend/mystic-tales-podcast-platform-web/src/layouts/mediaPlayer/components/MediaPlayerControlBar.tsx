import { FaBackward } from "react-icons/fa";
import { FaForward } from "react-icons/fa";

import { IoPlayCircle } from "react-icons/io5";
import { MdPauseCircleFilled } from "react-icons/md";

import { MdOutlineReplay10 } from "react-icons/md";
import { MdOutlineForward10 } from "react-icons/md";

import { IoIosHeartEmpty } from "react-icons/io";
import { IoMdHeart } from "react-icons/io";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { Skeleton } from "@/components/ui/skeleton";
import {
  pauseAudio,
  playAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useRef, useState, useEffect } from "react";

const MediaPlayerControl = () => {
  // REDUX
  const player = useSelector((state: RootState) => state.player);
  const user = useSelector((state: RootState) => state.auth.user);
  const dispatch = useDispatch();

  // REFS

  // STATES
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [isSeeking, setIsSeeking] = useState(false);

  // EFFECTS
  // Sync play/pause state with ReactPlayer
  // useEffect(() => {
  //   if (player.currentAudio && playerRef.current) {
  //     if (player.playMode.playStatus === "play") {
  //       // Seek to latest position when starting to play
  //       if (player.currentAudio.LatestPosition > 0 && currentTime === 0) {
  //         playerRef.current.seekTo(
  //           player.currentAudio.LatestPosition,
  //           "seconds"
  //         );
  //       }
  //     }
  //   }
  // }, [player.playMode.playStatus, player.currentAudio]);

  // FUNCTIONS
  const formatAudioLengthSmart = (audioLength: number): string => {
    const hours = Math.floor(audioLength / 3600);
    const minutes = Math.floor((audioLength % 3600) / 60);
    const seconds = Math.floor(audioLength % 60);

    const mm = minutes.toString().padStart(2, "0");
    const ss = seconds.toString().padStart(2, "0");

    return hours > 0
      ? `${hours.toString().padStart(2, "0")}:${mm}:${ss}`
      : `${mm}:${ss}`;
  };

  const handlePlayAudio = () => {
    dispatch(playAudio(null));
  };

  const handlePauseAudio = () => {
    dispatch(pauseAudio());
  };

  const handleProgress = (state: {
    played: number;
    playedSeconds: number;
    loaded: number;
    loadedSeconds: number;
  }) => {
    if (!isSeeking) {
      setCurrentTime(state.playedSeconds);
      // Có thể dispatch action để update LatestPosition trong Redux nếu cần
      // dispatch(updatePosition(state.playedSeconds));
    }
  };

  const handleDuration = (duration: number) => {
    setDuration(duration);
  };

  // const handleSeek = (e: React.MouseEvent<HTMLDivElement>) => {
  //   if (!playerRef.current || !player.currentAudio) return;

  //   const rect = e.currentTarget.getBoundingClientRect();
  //   const x = e.clientX - rect.left;
  //   const percentage = x / rect.width;
  //   const seekTime = percentage * (duration || player.currentAudio.AudioLength);

  //   playerRef.current.seekTo(seekTime, "seconds");
  //   setCurrentTime(seekTime);
  // };

  // const handleSkipBackward = () => {
  //   if (!playerRef.current) return;
  //   const newTime = Math.max(0, currentTime - 10);
  //   playerRef.current.seekTo(newTime, "seconds");
  //   setCurrentTime(newTime);
  // };

  // const handleSkipForward = () => {
  //   if (!playerRef.current || !player.currentAudio) return;
  //   const maxTime = duration || player.currentAudio.AudioLength;
  //   const newTime = Math.min(maxTime, currentTime + 10);
  //   playerRef.current.seekTo(newTime, "seconds");
  //   setCurrentTime(newTime);
  // };

  if (!user) {
    return (
      <div className="w-full h-full flex items-center px-5 relative">
        <div className="absolute inset-0 bg-black/50 "></div>
        <p className="text-gray-500 italic">Please Login To Listening!</p>
      </div>
    );
  }

  if (!player.currentAudio) {
    return (
      <div className="w-full h-full flex items-center px-5 relative">
        <div className="absolute inset-0 bg-black/50 "></div>

        <div className="flex items-center gap-3">
          <div className="bg-gray-500 w-12 h-12 rounded-md" />
          <div className="flex flex-col items-start justify-center">
            <p className="text-gray-400 font-semibold">No Audio Yet</p>
            <p className="text-gray-400 text-sm">
              You might need to play an audio to continue
            </p>
          </div>
        </div>

        <div className="flex items-center ml-20 gap-5">
          <div className="text-gray-400 cursor-not-allowed">
            <FaBackward size={20} />
          </div>
          <div className="text-gray-400 cursor-not-allowed">
            <IoPlayCircle size={40} />
          </div>
          <div className="text-gray-400 cursor-not-allowed">
            <FaForward size={20} />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full h-full flex items-center px-5">
      {/* Hidden ReactPlayer */}
      <div style={{ display: "none" }}></div>

      <div className="flex items-center gap-3">
        <img
          src={player.currentAudio.ImageUrl}
          className="w-12 h-12 aspect-square rounded-md shadow-md"
          alt={player.currentAudio.Name}
        />
        <div className="flex flex-col items-start justify-center w-[200px] overflow-ellipsis">
          <p className="text-white font-semibold line-clamp-1">
            {player.currentAudio.Name}
          </p>
          <p className="text-gray-200 text-sm line-clamp-1">
            {player.currentAudio.PodcasterName}
          </p>
        </div>
      </div>

      <div className="flex items-center ml-20 gap-5">
        <div className="text-white hover:text-mystic-green cursor-pointer">
          <FaBackward size={20} />
        </div>
        {player.playMode.playStatus === "pause" ? (
          <div
            onClick={handlePlayAudio}
            className="text-white hover:text-mystic-green cursor-pointer"
          >
            <IoPlayCircle size={50} />
          </div>
        ) : (
          <div
            onClick={handlePauseAudio}
            className="text-white hover:text-mystic-green cursor-pointer"
          >
            <MdPauseCircleFilled size={50} />
          </div>
        )}
        <div className="text-white hover:text-mystic-green cursor-pointer">
          <FaForward size={20} />
        </div>
      </div>

      <div className="hidden md:inline-flex flex-col md:w-[910px] items-center ml-20 gap-1">
        <div className="w-full relative flex items-center justify-start cursor-pointer">
          <div className="w-full h-1 rounded-full bg-gray-300/30"></div>
          <div
            style={{
              width: `${
                ((currentTime || player.currentAudio.LatestPosition) /
                  (duration || player.currentAudio.AudioLength)) *
                100
              }%`,
            }}
            className="absolute h-1 rounded-full bg-white z-10"
          ></div>
        </div>
        <div className="w-full relative flex items-center justify-between">
          <p
            style={{
              color: player.playMode.playStatus === "play" ? "#fff" : "#d1d5db",
            }}
            className="text-xs"
          >
            {formatAudioLengthSmart(
              currentTime || player.currentAudio.LatestPosition
            )}
          </p>
          <p
            style={{
              color:
                (currentTime || player.currentAudio.LatestPosition) ===
                (duration || player.currentAudio.AudioLength)
                  ? "#fff"
                  : "#d1d5db",
            }}
            className="text-xs"
          >
            {formatAudioLengthSmart(
              duration || player.currentAudio.AudioLength
            )}
          </p>
        </div>
      </div>
    </div>
  );
};

export default MediaPlayerControl;
