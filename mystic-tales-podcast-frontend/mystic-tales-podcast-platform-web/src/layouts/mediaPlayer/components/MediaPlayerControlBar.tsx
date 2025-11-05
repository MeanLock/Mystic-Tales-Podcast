import { FaBackward } from "react-icons/fa";
import { FaForward } from "react-icons/fa";

import { IoClose, IoPlayCircle } from "react-icons/io5";
import { MdOutlineFastRewind, MdPauseCircleFilled } from "react-icons/md";

import { MdOutlineReplay10 } from "react-icons/md";
import { MdOutlineForward10 } from "react-icons/md";

import { IoIosHeartEmpty } from "react-icons/io";
import { IoMdHeart } from "react-icons/io";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { Skeleton } from "@/components/ui/skeleton";
import {
  nextAudio,
  pauseAudio,
  playAudio,
  removeFromQueue,
  updateVolume,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useState } from "react";

import { MdOutlineQueueMusic } from "react-icons/md";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  FaRegCirclePlay,
  FaVolumeHigh,
  FaVolumeLow,
  FaVolumeXmark,
} from "react-icons/fa6";

import { Slider } from "@/components/ui/slider";
import { getAudioEngine } from "@/core/services/player/playerBridge";
import { useAudioProgress } from "@/core/services/player/useAudioPress";
import Waving from "@/components/loader/Waving";

const MediaPlayerControl = () => {
  // REDUX
  const player = useSelector((state: RootState) => state.player);
  const user = useSelector((state: RootState) => state.auth.user);
  const dispatch = useDispatch();

  // AUDIO ENGINE & PROGRESS
  const engine = getAudioEngine();
  const { currentTime: t, duration: d } = useAudioProgress(250);
  // REFS

  // STATES
  const [volume, setVolume] = useState<number>(player.playMode.volume);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [isSeeking, setIsSeeking] = useState(false);
  const [seekPreview, setSeekPreview] = useState<number | null>(null);

  const [isQueueModelOpen, setIsQueueModelOpen] = useState(false);
  const [isVolumeModelOpen, setIsVolumeModelOpen] = useState(false);

  const effectiveTime = isSeeking && seekPreview != null ? seekPreview : t;
  const effectiveDuration = d || player.currentAudio?.AudioLength || 0;

  const percent =
    effectiveDuration > 0 ? (effectiveTime / effectiveDuration) * 100 : 0;
  // EFFECTS

  // FUNCTIONS
  const onProgressMouse = (
    e: React.MouseEvent<HTMLDivElement, MouseEvent>,
    commit = false
  ) => {
    const bar = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - bar.left;
    const ratio = Math.min(1, Math.max(0, x / bar.width));
    const next = ratio * (effectiveDuration || 0);
    setSeekPreview(next);
    if (commit) {
      engine.seek(next);
      setIsSeeking(false);
      setSeekPreview(null);
    }
  };

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

  const handleUpdateVolume = (newVolume: number) => {
    setVolume(newVolume);
    // dispatch action to update volume in redux
    dispatch(updateVolume(newVolume));
  };

  const handleSeekBackward = () => {
    const newTime = Math.max(0, effectiveTime - 10);
    engine.seek(newTime);
  };

  const handleSeekForward = () => {
    const newTime = Math.min(effectiveDuration, effectiveTime + 10);
    engine.seek(newTime);
  };

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

        <div className="hidden md:inline-flex flex-col md:w-[800px] items-center ml-20 gap-1">
          <div className="w-full relative flex items-center justify-start cursor-pointer">
            <div className="w-full h-1 rounded-full bg-gray-300/30"></div>
          </div>
          <div className="w-full relative flex items-center justify-between">
            <p className="text-xs text-gray-400">00:00</p>
            <p className="text-xs text-gray-400">00:00</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full h-full flex items-center px-5">
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
        {/* <div className="text-white hover:text-mystic-green cursor-pointer">
          <FaBackward size={20} />
        </div> */}
        <div
          onClick={handleSeekBackward}
          className="text-white hover:text-mystic-green cursor-pointer"
        >
          <MdOutlineReplay10 size={20} />
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
        <div
          onClick={handleSeekForward}
          className="text-white hover:text-mystic-green cursor-pointer"
        >
          <MdOutlineForward10 size={20} />
        </div>
        {player.queueAudios.length > 0 && (
          <div
            onClick={() => dispatch(nextAudio())}
            className="text-white ml-10 hover:text-mystic-green cursor-pointer"
          >
            <FaForward size={30} />
          </div>
        )}
      </div>

      {/* Audio Length Tracking */}
      <div className="flex-1 hidden md:inline-flex flex-col md:w-[600px] items-center ml-20 gap-1">
        <div
          className="w-full relative flex items-center justify-start cursor-pointer"
          onMouseDown={(e) => {
            setIsSeeking(true);
            onProgressMouse(e, false);
          }}
          onMouseMove={(e) => isSeeking && onProgressMouse(e, false)}
          onMouseUp={(e) => onProgressMouse(e, true)}
          onMouseLeave={() => {
            if (isSeeking) {
              setIsSeeking(false);
              setSeekPreview(null);
            }
          }}
        >
          <div className="w-full h-1 rounded-full bg-gray-300/30"></div>
          <div
            style={{ width: `${percent}%` }}
            className="absolute h-1 rounded-full bg-white z-10"
          />
        </div>

        <div className="w-full relative flex items-center justify-between">
          <p
            className="text-xs"
            style={{
              color: player.playMode.playStatus === "play" ? "#fff" : "#d1d5db",
            }}
          >
            {formatAudioLengthSmart(effectiveTime)}
          </p>
          <p
            className="text-xs"
            style={{
              color:
                Math.floor(effectiveTime) === Math.floor(effectiveDuration)
                  ? "#fff"
                  : "#d1d5db",
            }}
          >
            {formatAudioLengthSmart(effectiveDuration)}
          </p>
        </div>
      </div>

      {/* Queue & Volume Management */}
      <div className="md:w-[200px] hidden md:inline-flex items-center justify-end gap-10">
        <div className="hidden md:inline-flex items-center justify-end">
          <Popover open={isQueueModelOpen} onOpenChange={setIsQueueModelOpen}>
            {/* chỉ icon mới toggle */}
            <PopoverTrigger asChild>
              <button
                className="
                  p-2 rounded-full cursor-pointer
                  bg-transparent hover:bg-gray-300/30 text-gray-300 hover:text-white
                  transition ease-out duration-300
                "
                aria-label="Open queue"
              >
                <MdOutlineQueueMusic size={25} />
              </button>
            </PopoverTrigger>

            <PopoverContent
              side="top" // mở phía trên icon
              align="end" // mép phải bám icon (kiểu chatbot)
              sideOffset={12} // cách icon 12px
              collisionPadding={8}
              className="
                w-96 h-96 rounded-2xl shadow-2xl
                bg-black/40 backdrop-blur-md border border-white/10
                text-white p-3
                flex flex-col items-start gap-3
              "
              // ⛔ không đóng khi click ra vùng body: chỉ icon mới toggle
              onInteractOutside={(e) => e.preventDefault()}
            >
              <p className="font-poppins text-xs text-neutral-400">Current</p>
              {/* Current Audio */}
              <div className="w-full flex items-center">
                <div className="flex items-center justify-center relative">
                  <img
                    src={player.currentAudio.ImageUrl}
                    className="w-12 h-12 aspect-square rounded-md shadow-md"
                    alt={player.currentAudio.Name}
                  />

                  {player.playMode.playStatus === "play" ? (
                    <div
                      onClick={() => dispatch(pauseAudio())}
                      className="absolute w-12 h-12 flex items-center justify-center inset-0"
                    >
                      <Waving />
                    </div>
                  ) : (
                    <div
                      onClick={() => dispatch(playAudio(null))}
                      className="bg-black/30 absolute w-12 h-12 flex items-center justify-center inset-0"
                    >
                      <FaRegCirclePlay size={25} color="#fff" />
                    </div>
                  )}
                </div>

                <div className="flex flex-col items-start justify-center ml-3 overflow-ellipsis">
                  <p className="text-white font-semibold line-clamp-1">
                    {player.currentAudio.Name}
                  </p>
                  <p className="text-sm text-white font-light line-clamp-1">
                    {player.currentAudio.PodcasterName}
                  </p>
                </div>
              </div>
              <div className="w-full h-[0.3px] bg-neutral-400" />

              <p className="font-poppins text-xs text-neutral-400">Queue</p>
              <div
                className="w-full md:h-[230px] overflow-y-scroll flex flex-col items-center gap-2  [&::-webkit-scrollbar]:hidden
                [-ms-overflow-style:none]
                [scrollbar-width:none]"
              >
                {player.queueAudios.length === 0 ? (
                  <p className="text-gray-400 italic text-sm">
                    No audio in queue
                  </p>
                ) : (
                  player.queueAudios.map((audio) => (
                    <div
                      key={`queue-audio-${audio.Id}-${audio.Index}`}
                      className="w-full flex items-center transition-all hover:bg-white/10 p-1 rounded-md cursor-pointer"
                    >
                      <img
                        src={audio.ImageUrl}
                        className="w-10 h-10 aspect-square rounded-md shadow-md"
                        alt={audio.Name}
                      />
                      <div className="flex flex-col items-start justify-center ml-3 w-2/3 overflow-ellipsis">
                        <p className="text-white font-semibold line-clamp-1">
                          {audio.Name}
                        </p>
                        <p className="text-sm text-white font-light line-clamp-1">
                          {audio.PodcasterName}
                        </p>
                      </div>
                      <div
                        onClick={() =>
                          dispatch(
                            removeFromQueue({
                              id: audio.Id,
                              index: audio.Index,
                            })
                          )
                        }
                        className="flex-1 flex items-center justify-end pr-3 text-neutral-400 hover:text-white cursor-pointer"
                      >
                        <IoClose size={15} />
                      </div>
                    </div>
                  ))
                )}
              </div>
            </PopoverContent>
          </Popover>
        </div>

        <div className="hidden md:inline-flex items-center justify-center">
          <Popover open={isVolumeModelOpen} onOpenChange={setIsVolumeModelOpen}>
            {/* chỉ icon mới toggle */}
            <PopoverTrigger asChild>
              {volume === 0 ? (
                <button
                  className="
              p-2 rounded-full cursor-pointer
              bg-transparent hover:bg-gray-300/30 text-gray-300 hover:text-white
              transition ease-out duration-300
            "
                  aria-label="Open queue"
                >
                  <FaVolumeXmark size={25} />
                </button>
              ) : volume < 51 && volume > 0 ? (
                <button
                  className="
              p-2 rounded-full cursor-pointer
              bg-transparent hover:bg-gray-300/30 text-gray-300 hover:text-white
              transition ease-out duration-300
            "
                  aria-label="Open queue"
                >
                  <FaVolumeLow size={25} />
                </button>
              ) : (
                <button
                  className="
              p-2 rounded-full cursor-pointer
              bg-transparent hover:bg-gray-300/30 text-gray-300 hover:text-white
              transition ease-out duration-300
            "
                  aria-label="Open queue"
                >
                  <FaVolumeHigh size={25} />
                </button>
              )}
            </PopoverTrigger>

            <PopoverContent
              side="top" // mở phía trên icon
              align="center" // mép phải bám icon (kiểu chatbot)
              sideOffset={12} // cách icon 12px
              collisionPadding={0}
              className="
                w-18 h-56 rounded-md shadow-2xl
                bg-black/40 backdrop-blur-md border border-white/10
                text-white p-3
                flex items-center justify-center
              "
            >
              <Slider
                defaultValue={[volume]}
                max={100}
                inverted
                value={[100 - volume]} // hiển thị ngược
                onValueChange={([v]) => {
                  handleUpdateVolume(100 - v);
                }} // kéo lên => volume tăng
                step={1}
                orientation="vertical"
                className="h-40"
              />
              {/* Queue content here */}
            </PopoverContent>
          </Popover>
        </div>
      </div>
    </div>
  );
};

export default MediaPlayerControl;
