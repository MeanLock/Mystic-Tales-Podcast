// @ts-nocheck
import { FaBackward } from "react-icons/fa";
import { FaForward } from "react-icons/fa";

import { IoPlayCircle } from "react-icons/io5";
import { MdPauseCircleFilled } from "react-icons/md";

import { MdOutlineReplay10 } from "react-icons/md";
import { MdOutlineForward10 } from "react-icons/md";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import {
  pauseAudio,
  playAudio,
  setUIIsAutoPlay,
  setUIPlayOrderMode,
  setVolume,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useEffect, useState, useMemo } from "react";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { FaVolumeHigh, FaVolumeLow, FaVolumeXmark } from "react-icons/fa6";

import { Slider } from "@/components/ui/slider";
// import { getAudioEngine } from "@/core/services/player/playerBridge";
// import { useAudioProgress } from "@/core/services/player/useAudioPress";
import { Switch } from "@/components/ui/switch";
import { Repeat, Shuffle } from "lucide-react";
import { useUpdatePlayModeMutation } from "@/core/services/player/player.service";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import ResolvedImage from "./ResolvedImage";
import { usePlayer } from "@/core/services/player/usePlayer";
import { CgSpinner } from "react-icons/cg";

const MediaPlayerControl = () => {
  // REDUX
  const player = useSelector((state: RootState) => state.player);
  const user = useSelector((state: RootState) => state.auth.user);
  const dispatch = useDispatch();

  // NEW CONCEPT
  const {
    state,
    play,
    pause,
    seek,
    navigateInSpecifyShow,
    navigateInSavedEpisodes,
    navigateInBookingTracks,
  } = usePlayer();

  // MUTATIONS
  const [updatePlayMode] = useUpdatePlayModeMutation();

  // AUDIO ENGINE & PROGRESS
  const engine = getAudioEngine();
  const { currentTime: t, duration: d } = useAudioProgress(250);
  // REFS

  // STATES
  const [volume, setVolumeState] = useState<number>(player.playMode.volume);
  const [isSeeking, setIsSeeking] = useState(false);
  const [seekPreview, setSeekPreview] = useState<number | null>(null);
  const [isInitialized, setIsInitialized] = useState(false);
  const [isVolumeModelOpen, setIsVolumeModelOpen] = useState(false);

  const effectiveTime = isSeeking && seekPreview != null ? seekPreview : t;
  const effectiveDuration = d || player.currentAudio?.AudioLength || 0;

  // NEW CONCEPT
  const percent = state.duration > 0 ? (state.currentTime / state.duration) * 100 : 0;

const handleProgressClick = (e: any) => {
  const rect = e.currentTarget.getBoundingClientRect();
  const x = e.clientX - rect.left;
  const ratio = x / rect.width;
  seek(ratio * state.duration);
};

  // const percent =
  //   effectiveDuration > 0 ? (effectiveTime / effectiveDuration) * 100 : 0;

  // EFFECTS
  // Khởi tạo ban đầu từ listenSessionProcedure, sau đó theo playMode
  useEffect(() => {
    if (!isInitialized && player.listenSessionProcedure) {
      // Chỉ set lần đầu từ listenSessionProcedure
      dispatch(setUIIsAutoPlay(player.listenSessionProcedure.IsAutoPlay));
      dispatch(setUIPlayOrderMode(player.listenSessionProcedure.PlayOrderMode));
      setIsInitialized(true);
    }
  }, [player.listenSessionProcedure, isInitialized, dispatch]);

  // Lấy giá trị từ playMode (Redux state)
  const isAutoPlay = player.playMode.isAutoPlay;
  const playOrderMode = player.playMode.nextMode;

  // FUNCTIONS
  // const onProgressMouse = (
  //   e: React.MouseEvent<HTMLDivElement, MouseEvent>,
  //   commit = false
  // ) => {
  //   const bar = e.currentTarget.getBoundingClientRect();
  //   const x = e.clientX - bar.left;
  //   const ratio = Math.min(1, Math.max(0, x / bar.width));
  //   const next = ratio * (effectiveDuration || 0);
  //   setSeekPreview(next);
  //   if (commit) {
  //     engine.seek(next);
  //     setIsSeeking(false);
  //     setSeekPreview(null);
  //   }
  // };

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

  // NEW CONCEPT
  const handlePlayPause = () => {
    if (state.isPlaying) pause();
    else play();
  };

  const handlePlayAudio = () => {
    dispatch(playAudio(null));
  };

  const handlePauseAudio = () => {
    dispatch(pauseAudio());
  };

  // NEW CONCEPT
  const handleVolume = (v: number) => setVolume(v);

  // const handleUpdateVolume = (newVolume: number) => {
  //   setVolumeState(newVolume);
  //   dispatch(setVolume(newVolume));
  // };

  // const handleSeekBackward = () => {
  //   const newTime = Math.max(0, effectiveTime - 10);
  //   engine.seek(newTime);
  // };

  // const handleSeekForward = () => {
  //   const newTime = Math.min(effectiveDuration, effectiveTime + 10);
  //   engine.seek(newTime);
  // };

  // NEW CONCEPT
  const handleSeekForward = () => seek(state.currentTime + 10);
  const handleSeekBackward = () => seek(state.currentTime - 10);

  // NEW CONCEPT
  // const handleNext = () => {
  //   if (state.sourceType === "SpecifyShowEpisodes")
  //     const benefitListNe = []; // Nếu có benefitList thì truyền vào đây
  //     navigateInSpecifyShow({ benefitList: benefitListNe,navigateType: "Next" });
  //   else if (state.sourceType === "SavedEpisodes")
  //     navigateInSavedEpisodes({ navigateType: "Next" });
  //   else navigateInBookingTracks({ navigateType: "Next" });
  // };

  // const handleNextAudio = () => {
  //   engine.next?.();
  // };

  const handlePreviousAudio = () => {
    engine.previous?.();
  };

  // Kiểm tra xem có nên disable next/previous không
  const isNavigationDisabled = useMemo(() => {
    if (!player.listenSessionProcedure) return true;

    // Nếu isNextSessionNull là true, disable
    if (player.playMode.isNextSessionNull) return true;

    if (!player.listenSession) return true;

    // Lấy playOrder dựa trên playMode.listenSessionProcedure (Redux state)
    const playOrder =
      player.listenSessionProcedure.PlayOrderMode === "Sequential"
        ? player.listenSessionProcedure.ListenObjectsSequentialOrder
        : player.listenSessionProcedure.ListenObjectsRandomOrder;

    console.log("PlayOrder for disable check:", playOrder);
    console.log("Using nextMode:", player.listenSessionProcedure.PlayOrderMode);

    // Đếm số item IsListenable
    const listenableCount =
      playOrder?.filter((item) => item.IsListenable).length || 0;

    console.log("Listenable count:", listenableCount);

    // Disable nếu có <= 1 item
    return listenableCount <= 1;
  }, [player.listenSessionProcedure]);

  const handleChangeOrderMode = async (mode: "Sequential" | "Random") => {
    if (!player.listenSessionProcedure?.Id) {
      dispatch(
        setError({
          message: "No active session to update play mode",
          autoClose: 5,
        })
      );
      return;
    }

    // Lưu giá trị cũ để revert nếu cần
    const previousMode = playOrderMode;

    // Optimistic UI update
    dispatch(setUIPlayOrderMode(mode));
    try {
      await updatePlayMode({
        PlayOrderMode: mode,
        IsAutoPlay: isAutoPlay,
        CustomerListenSessionProcedureId: player.listenSessionProcedure.Id,
      }).unwrap();
    } catch (error) {
      console.error("Failed to update play order mode:", error);
      // Revert on error
      dispatch(setUIPlayOrderMode(previousMode));
      dispatch(
        setError({
          message: "Failed to update play order mode. Please try again.",
          autoClose: 5,
        })
      );
    }
  };

  const handleChangeAutoPlay = async (checked: boolean) => {
    if (!player.listenSessionProcedure?.Id) {
      dispatch(
        setError({
          message: "No active session to update autoplay",
          autoClose: 5,
        })
      );
      return;
    }

    // Lưu giá trị cũ để revert nếu cần
    const previousAutoPlay = isAutoPlay;

    // Optimistic UI update
    dispatch(setUIIsAutoPlay(checked));

    try {
      await updatePlayMode({
        PlayOrderMode: playOrderMode,
        IsAutoPlay: checked,
        CustomerListenSessionProcedureId: player.listenSessionProcedure.Id,
      }).unwrap();
    } catch (error) {
      console.error("Failed to update autoplay mode:", error);
      // Revert on error
      dispatch(setUIIsAutoPlay(previousAutoPlay));
      dispatch(
        setError({
          message: "Failed to update autoplay mode. Please try again.",
          autoClose: 5,
        })
      );
    }
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
          <div className="bg-gray-500 w-12 aspect-square rounded-md" />
          <div className="flex flex-col items-start justify-center ">
            <p className="text-gray-400 font-semibold line-clamp-1">
              No Audio Yet
            </p>
            <p className="text-gray-400 text-sm line-clamp-1">
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
        <ResolvedImage
          MainImageFileKey={player.currentAudio.MainImageFileKey}
          Name={player.currentAudio.Name}
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
        <div
          onClick={isNavigationDisabled ? undefined : handlePreviousAudio}
          className={`${
            isNavigationDisabled
              ? "text-gray-500 cursor-not-allowed"
              : "text-white hover:text-mystic-green cursor-pointer"
          }`}
        >
          <FaBackward size={20} />
        </div>
        <div
          onClick={handleSeekBackward}
          className="text-white hover:text-mystic-green cursor-pointer"
        >
          <MdOutlineReplay10 size={20} />
        </div>
        {/* NEW CONCEPT */}
        {state.buffering ? (
          <CgSpinner />
        ) : state.isPlaying ? (
          <MdPauseCircleFilled size={50} onClick={handlePlayPause} />
        ) : (
          <IoPlayCircle size={50} onClick={handlePlayPause} />
        )}

        {/* {player.isBuffering ? (
          <div className="text-white flex items-center justify-center">
            <svg
              className="h-[45px] w-[45px] text-white animate-[rotate-spinner_1s_linear_infinite]"
              viewBox="0 0 24 24"
              fill="none"
            >
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="2"
              />
              <path
                className="opacity-80"
                fill="none"
                stroke="currentColor"
                strokeLinecap="round"
                strokeWidth="2" // giảm xuống 1.5 hoặc 1 nếu muốn mỏng nữa
                d="M12 2a10 10 0 0 1 10 10" // một cung tròn từ trên xuống bên phải
              />
            </svg>
          </div>
        ) : player.playMode.playStatus === "pause" ? (
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
        )} */}
        <div
          onClick={handleSeekForward}
          className="text-white hover:text-mystic-green cursor-pointer"
        >
          <MdOutlineForward10 size={20} />
        </div>
        <div
          onClick={isNavigationDisabled ? undefined : handleNextAudio}
          className={`${
            isNavigationDisabled
              ? "text-gray-500 cursor-not-allowed"
              : "text-white hover:text-mystic-green cursor-pointer"
          }`}
        >
          <FaForward size={20} />
        </div>
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

      {/* Listen Mode Management */}
      <div className="flex items-center justify-end gap-3 p-2 ml-7">
        {/* IsAutoPlay */}
        {!isNavigationDisabled && (
          <div className="flex items-center space-x-2">
            {isAutoPlay ? (
              <p className="text-mystic-green font-poppins font-bold text-xs">
                Autoplay
              </p>
            ) : (
              <p className="text-[#D9D9D9] font-poppins font-bold text-xs">
                Autoplay
              </p>
            )}
            <Switch
              checked={isAutoPlay}
              onCheckedChange={handleChangeAutoPlay}
              id="is-auto-play-mode"
              className="
          data-[state=checked]:bg-[#aee339]   /* màu nền khi bật */
          data-[state=unchecked]:bg-[#d9d9d9] /* tuỳ chọn: màu khi tắt */
          "
            />
          </div>
        )}

        {/* Sequential/Random */}
        <div className="flex items-center justify-end mx-3 gap-3">
          {/* Herre */}
          <div
            onClick={() => handleChangeOrderMode("Random")}
            className={`p-2 transition-all duration-500 hover:scale-110 cursor-pointer rounded-md text-[#D9D9D9] flex items-center justify-center ${
              playOrderMode === "Random"
                ? "text-white bg-white/20"
                : "bg-transparent hover:bg-white/10 hover:text-white"
            }`}
          >
            <Shuffle size={15} />
          </div>
          <div
            onClick={() => handleChangeOrderMode("Sequential")}
            className={`p-2 transition-all duration-500 hover:scale-110 cursor-pointer rounded-md text-[#D9D9D9] flex items-center justify-center ${
              playOrderMode === "Sequential"
                ? "text-white bg-white/20"
                : "bg-transparent hover:bg-white/10 hover:text-white"
            }`}
          >
            <Repeat size={15} />
          </div>
        </div>
      </div>

      {/* Volume Management */}
      <div className="md:w-[200px] hidden md:inline-flex items-center justify-end gap-10">
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
