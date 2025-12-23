import { usePlayer } from "@/core/services/player/usePlayer";
import type { RootState } from "@/redux/store";
import { FaBackward, FaForward } from "react-icons/fa";
import { IoPauseCircle, IoPlayCircle } from "react-icons/io5";
import { useDispatch, useSelector } from "react-redux";
import { useEffect, useRef, useState, useCallback } from "react";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { getPlayerController } from "@/core/services/player/playerController";
import type {
  ListenSessionBookingTracks,
  ListenSessionEpisodes,
} from "@/core/types/audio";
import {
  useUpdateBookingTrackLastDurationMutation,
  useUpdateEpisodeLastDurationMutation,
} from "@/core/services/player/player.service";
import {
  useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery,
  type SubscriptionBenefit,
} from "@/core/services/subscription/subscription.service";
import { TbRepeat } from "react-icons/tb";
import { Shuffle, Volume1, Volume2, VolumeX } from "lucide-react";
import { MdAutoMode } from "react-icons/md";
import { useLazyCheckUserPodcastListenSlotQuery } from "@/core/services/account/account.service";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import { Slider } from "@/components/ui/slider";

const NewMediaPlayerControlBar = () => {
  // REDUX
  const user = useSelector((state: RootState) => state.auth.user);
  const playerState = useSelector((state: RootState) => state.player);

  // PLAYER CORE
  const {
    play,
    pause,
    stop,
    seek,
    setVolume,
    handleUpdatePlayMode,
    navigateInSpecifyShow,
    navigateInBookingTracks,
    navigateInSavedEpisodes,
    state: playerUiState,
  } = usePlayer();

  const controller = getPlayerController();

  // Use state from usePlayer hook for reactive updates
  const state = playerUiState;
  const dispatch = useDispatch();

  const [triggerGetSubscriptionBenefitsMapListFromEpisodeId] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();
  const [triggerCheckPodcastListenSlot] =
    useLazyCheckUserPodcastListenSlotQuery();

  // STATES
  const [playOrderMode, setPlayOrderMode] = useState<"Sequential" | "Random">(
    "Sequential"
  );
  const [isAutoPlay, setIsAutoPlay] = useState<boolean>(false);
  const [sourceType, setSourceType] = useState<
    "SpecifyShowEpisodes" | "SavedEpisodes" | "BookingProducingTracks"
  >("SpecifyShowEpisodes");
  const [isNavigable, setIsNavigable] = useState<boolean>(false);

  const handleNavigate = useCallback(
    async (navigateType: "Next" | "Previous") => {
      // Logic để chuyển về audio trước đó
      // Khử null
      if (
        !playerState.listenSessionProcedure ||
        !isNavigable ||
        !state.currentAudio
      )
        return;

      // Xử lý logic chuyển audio dựa trên playOrderMode
      if (sourceType === "SpecifyShowEpisodes") {
        // Kiểm tra subscription benefits và listen slot trước khi cho phép chuyển
        const benefitsData =
          await triggerGetSubscriptionBenefitsMapListFromEpisodeId({
            PodcastEpisodeId: state.currentAudio.id,
          }).unwrap();
        const podcastListenSlot =
          await triggerCheckPodcastListenSlot().unwrap();
        const benefitList =
          benefitsData.CurrentPodcastSubscriptionRegistrationBenefitList;

        if (benefitList.length > 0) {
          const hasNonQuotaBenefit = benefitList.some((b) => b.Id === 1);
          if (hasNonQuotaBenefit) {
            // Cho đi tiếp
            await navigateInSpecifyShow({
              benefitList: benefitList,
              navigateType: navigateType,
            });
          } else {
            if (podcastListenSlot <= 0) {
              // Không đủ slot để nghe
              dispatch(
                setError({
                  message:
                    "You have no podcast listen slot left. Please subscribe to a plan to get more listen slots.",
                  autoClose: 10,
                })
              );
              pause();
              return;
            }
          }
        } else {
          if (podcastListenSlot <= 0) {
            // Không đủ slot để nghe
            dispatch(
              setError({
                message:
                  "You have no podcast listen slot left. Please subscribe to a plan to get more listen slots.",
                autoClose: 10,
              })
            );
            pause();
            return;
          } else {
            // Cho đi tiếp
            await navigateInSpecifyShow({
              benefitList: benefitList,
              navigateType: navigateType,
            });
          }
        }
      } else if (sourceType === "SavedEpisodes") {
        await navigateInSavedEpisodes({ navigateType: navigateType });
      } else {
        await navigateInBookingTracks({ navigateType: navigateType });
      }
    },
    [
      playerState.listenSessionProcedure,
      isNavigable,
      state.currentAudio,
      sourceType,
      navigateInSpecifyShow,
      navigateInSavedEpisodes,
      navigateInBookingTracks,
      triggerGetSubscriptionBenefitsMapListFromEpisodeId,
      triggerCheckPodcastListenSlot,
      dispatch,
      pause,
    ]
  );

  // REFS
  const currentTimeRef = useRef(0);

  useEffect(() => {
    currentTimeRef.current = state.currentTime;
  }, [state.currentTime]);

  // Subscribe to ended event from playerController (won't override usePlayer's listeners now)
  useEffect(() => {
    const unsubscribe = controller.attachEvents({
      onEnded: async () => {
        console.log(
          "Audio ended, isNavigable:",
          isNavigable,
          "isAutoPlay:",
          isAutoPlay
        );
        // UPDATE LAST DURATION
        await updateLastDurationOnce(state.duration);
        if (isNavigable && isAutoPlay) {
          await handleNavigate("Next");
        }
      },
    });
    return unsubscribe;
  }, [controller, isNavigable, isAutoPlay, handleNavigate]);

  // RTK Query
  const [updateEpisodeLastDuration] = useUpdateEpisodeLastDurationMutation();
  const [updateBookingTrackLastDuration] =
    useUpdateBookingTrackLastDurationMutation();

  // STATES
  const [currentPosition, setCurrentPosition] = useState(0); // in seconds
  const [isSeeking, setIsSeeking] = useState(false);
  const percent =
    state.duration > 0
      ? ((isSeeking ? currentPosition : state.currentTime) / state.duration) *
        100
      : 0;

  const fmt = (t: number) => {
    const m = Math.floor(t / 60);
    const s = Math.floor(t % 60);
    return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
  };

  // EFFECTS
  useEffect(() => {
    if (!playerState || !playerState.listenSessionProcedure) {
      console.log("Player state or listen session procedure is missing.");
      setIsAutoPlay(false);
      setPlayOrderMode("Sequential");
      setSourceType("SpecifyShowEpisodes");
      setIsNavigable(false);
      return;
    } else {
      console.log("Player state and listen session procedure:", playerState);
      setIsAutoPlay(playerState.listenSessionProcedure.IsAutoPlay);
      setPlayOrderMode(playerState.listenSessionProcedure.PlayOrderMode);
      setSourceType(playerState.listenSessionProcedure.SourceDetail.Type);
      const procedure = playerState.listenSessionProcedure;
      if (procedure.PlayOrderMode === "Sequential") {
        if (!procedure.ListenObjectsSequentialOrder) {
          setIsNavigable(false);
          return;
        }
        const availableCount = procedure.ListenObjectsSequentialOrder.filter(
          (order) => order.IsListenable === true
        ).length;
        if (availableCount <= 1) {
          setIsNavigable(false);
          return;
        } else {
          setIsNavigable(true);
          return;
        }
      } else {
        if (!procedure.ListenObjectsRandomOrder) {
          setIsNavigable(false);
          return;
        }
        const availableCount = procedure.ListenObjectsRandomOrder.filter(
          (order) => order.IsListenable === true
        ).length;
        if (availableCount <= 1) {
          setIsNavigable(false);
          return;
        } else {
          setIsNavigable(true);
          return;
        }
      }
    }
  }, [playerState, playerState.listenSessionProcedure]);

  // Cập nhật latest listen duration mỗi 2 giây khi đang phát
  useEffect(() => {
    if (!state.isPlaying || !state.currentAudio || !state.currentAudio.id) {
      console.log("Not playing or no current audio, skipping interval.");
      return;
    }

    const interval = setInterval(async () => {
      const latest = Math.floor(currentTimeRef.current);
      console.log("Updating latest listen duration to:", playerState);
      const sourceType = playerState.listenSessionProcedure?.SourceDetail.Type;

      if (!state.currentAudio) {
        clearInterval(interval);
        return;
      }
      if (sourceType !== "BookingProducingTracks") {
        // Gọi API cập nhật latest listen duration cho episode
        let benefitData: SubscriptionBenefit[] = [];
        const repsponse =
          await triggerGetSubscriptionBenefitsMapListFromEpisodeId({
            PodcastEpisodeId: state.currentAudio.id,
          }).unwrap();
        benefitData =
          repsponse.CurrentPodcastSubscriptionRegistrationBenefitList;
        try {
          const sessionId = (playerState.listenSession as ListenSessionEpisodes)
            .PodcastEpisodeListenSession.Id;
          await updateEpisodeLastDuration({
            PodcastEpisodeListenSessionId: sessionId,
            LastListenDurationSeconds: latest,
            CurrentPodcastSubscriptionRegistrationBenefitList: benefitData,
          }).unwrap();
        } catch (error) {
          console.error("Failed to update episode last duration:", error);
          // Kill playback session - user không được phép nghe nữa
          stop();
          clearInterval(interval);
          dispatch(
            setError({
              message:
                "Your listening session has ended. Please start a new session to continue.",
              autoClose: 10,
            })
          );
        }
      } else {
        // Gọi API cập nhật latest listen duration cho booking track
        try {
          const sessionId = (
            playerState.listenSession as ListenSessionBookingTracks
          ).BookingPodcastTrackListenSession.Id;
          await updateBookingTrackLastDuration({
            BookingPodcastTrackListenSessionId: sessionId,
            LastListenDurationSeconds: latest,
          }).unwrap();
        } catch (error) {
          console.error("Failed to update booking track last duration:", error);
          // Kill playback session - user không được phép nghe nữa
          stop();
          clearInterval(interval);
          dispatch(
            setError({
              message:
                "Your listening session has ended. Please start a new session to continue.",
              autoClose: 10,
            })
          );
        }
      }
    }, 2000);

    return () => clearInterval(interval);
  }, [state.isPlaying, state.currentAudio?.id]);

  // Helper: cập nhật ngay lập tức latest duration (dùng sau khi seek)
  const updateLastDurationOnce = async (durationSeconds: number) => {
    try {
      const sessionId = (playerState.listenSession as ListenSessionEpisodes)
        .PodcastEpisodeListenSession?.Id;
      const sourceType = playerState.listenSessionProcedure?.SourceDetail.Type;

      if (!state.currentAudio || !state.currentAudio.id) return;

      if (sourceType !== "BookingProducingTracks") {
        let benefitData: SubscriptionBenefit[] = [];
        const repsponse =
          await triggerGetSubscriptionBenefitsMapListFromEpisodeId({
            PodcastEpisodeId: state.currentAudio.id,
          }).unwrap();
        benefitData =
          repsponse.CurrentPodcastSubscriptionRegistrationBenefitList;
        await updateEpisodeLastDuration({
          PodcastEpisodeListenSessionId: sessionId,
          LastListenDurationSeconds: Math.floor(durationSeconds),
          CurrentPodcastSubscriptionRegistrationBenefitList: benefitData,
        }).unwrap();
      } else {
        const bookingSessionId = (playerState.listenSession as any)
          ?.BookingPodcastTrackListenSession?.Id;
        await updateBookingTrackLastDuration({
          BookingPodcastTrackListenSessionId: bookingSessionId,
          LastListenDurationSeconds: Math.floor(durationSeconds),
        }).unwrap();
      }
    } catch (error) {
      console.error("Failed to update last duration immediately:", error);
    }
  };

  // FUNCTIONS

  const handlePlay = () => {
    play();
  };

  const handlePause = () => {
    pause();
  };

  // SEEK HANDLERS
  const barRef = useRef<HTMLDivElement | null>(null);

  const onSeekStart = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!state.duration) return;
    e.preventDefault();
    e.stopPropagation();
    setIsSeeking(true);
    updateSeekByEvent(e);
    // Attach listeners to window for smooth dragging
    window.addEventListener("mousemove", onSeekMove as any);
    window.addEventListener("mouseup", onSeekEnd as any, { once: true });
  };

  const onSeekMove = (e: MouseEvent) => {
    const pos = computeSeekByClient(e.clientX);
    if (typeof pos === "number") setCurrentPosition(pos);
  };

  const onSeekEnd = (e: MouseEvent) => {
    // Compute final target directly to avoid stale state
    const finalPos = computeSeekByClient(e.clientX);
    if (typeof finalPos === "number") {
      setCurrentPosition(finalPos);
      if (state.duration) {
        console.log("Seeking to at front-end:", Math.max(0, finalPos));
        seek(finalPos);
        // Cập nhật ngay latest duration sau khi seek
        updateLastDurationOnce(finalPos);
      }
    }
    setIsSeeking(false);
    window.removeEventListener("mousemove", onSeekMove as any);
  };

  const updateSeekByEvent = (e: React.MouseEvent<HTMLDivElement>) => {
    const pos = computeSeekByClient(e.clientX);
    if (typeof pos === "number") setCurrentPosition(pos);
  };

  const computeSeekByClient = (clientX: number) => {
    const bar = barRef.current;
    if (!bar || !state.duration) return;
    const rect = bar.getBoundingClientRect();
    const x = Math.min(Math.max(clientX - rect.left, 0), rect.width);
    const ratio = x / rect.width;
    return ratio * state.duration;
  };

  const handleChangePlayOrderMode = async (mode: "Sequential" | "Random") => {
    console.log("Changing play order mode to:", mode);
    const prev = playOrderMode;
    setPlayOrderMode(mode);
    try {
      await handleUpdatePlayMode({
        change: "OrderMode",
        PlayOrderMode: mode,
      });
    } catch (error) {
      setPlayOrderMode(prev);
    }
    // Gọi API hoặc dispatch action để lưu thay đổi chế độ chơi
  };

  const handleChangeIsAutoPlay = async (autoPlay: boolean) => {
    console.log("Changing auto play to:", autoPlay);
    const prev = isAutoPlay;
    setIsAutoPlay(autoPlay);
    try {
      await handleUpdatePlayMode({
        change: "AutoPlay",
        IsAutoPlay: autoPlay,
      });
    } catch (error) {
      setIsAutoPlay(prev);
    }
    // Gọi API hoặc dispatch action để lưu thay đổi chế độ tự động phát
  };

  // RENDER
  if (!user) {
    return (
      <div className="w-full h-full flex items-center px-5 relative">
        <div className="absolute inset-0 bg-black/50 "></div>
        <p className="text-gray-500 italic">Please Login To Listening!</p>
      </div>
    );
  }

  if (!state.currentAudio) {
    return (
      <div className="w-full h-full flex items-center px-5 relative">
        <div className="absolute inset-0 bg-black/50 "></div>

        <div className="flex items-center gap-3">
          <div className="bg-gray-500 w-12 aspect-square rounded-full shadow-sm" />
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
            <IoPlayCircle size={50} />
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
    <div className="w-full h-full flex items-center px-5 gap-5 md:gap-10">
      {/* AUDIO INFORMATIONS */}
      <div className="flex items-center gap-2">
        <div className="flex items-center justify-center w-15">
          <AutoResolveImage
            FileKey={state.currentAudio.image || ""}
            Name={state.currentAudio.name}
            type={
              state.sourceType === "BookingProducingTracks"
                ? "BookingPublicSource"
                : "PodcastPublicSource"
            }
            imgClassName="rounded-full w-12 h-12 object-cover aspect-square"
            key={state.currentAudio.id}
          />
        </div>
        <div className="flex flex-col items-start justify-center ">
          <p className="text-white font-semibold line-clamp-1">
            {state.currentAudio.name}
          </p>
          <p className="text-gray-400 text-sm line-clamp-1">
            {state.currentAudio.podcasterName}
          </p>
        </div>
      </div>
      {/* CONTROLS */}
      <div className="flex items-center justify-center gap-5">
        <div
          className={`
            hidden md:inline-flex items-center justify-center 
            text-gray-400 
            ${
              isNavigable
                ? "cursor-pointer hover:text-mystic-green transition-colors duration-200"
                : "cursor-not-allowed"
            }
          `}
          onClick={() => handleNavigate("Previous")}
        >
          <FaBackward size={20} />
        </div>

        {state.buffering || state.seeking ? (
          // Đây là biểu tượng loading khi đang buffering hoặc seeking
          <div
            className={`
            flex items-center justify-center 
            text-gray-400 cursor-not-allowed
            hover:scale-110 transition-all duration-500
        `}
            onClick={() => handlePause()}
          >
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
                strokeWidth="2"
                d="M12 2a10 10 0 0 1 10 10"
              />
            </svg>
          </div>
        ) : state.isPlaying ? (
          <div
            className={`
            flex items-center justify-center 
            text-mystic-green cursor-pointer
            hover:scale-110 transition-all duration-500
        `}
            onClick={() => handlePause()}
          >
            <IoPauseCircle size={50} />
          </div>
        ) : (
          <div
            className="
            flex items-center justify-center 
            text-mystic-green cursor-pointer 
            hover:scale-110 transition-all duration-500
        "
            onClick={() => handlePlay()}
          >
            <IoPlayCircle size={50} />
          </div>
        )}

        <div
          className={`
            hidden md:inline-flex items-center justify-center 
            text-gray-400 
            ${
              isNavigable
                ? "cursor-pointer hover:text-mystic-green transition-colors duration-200"
                : "cursor-not-allowed"
            }
          `}
          onClick={() => handleNavigate("Next")}
        >
          <FaForward size={20} />
        </div>
      </div>

      {/* SEEK BAR */}
      <div className="flex flex-col w-[200px] md:w-[800px] items-end gap-1">
        <div
          ref={barRef}
          className="w-full relative flex items-center justify-start cursor-pointer"
          onMouseDown={onSeekStart}
        >
          <div className="w-full h-1 rounded-full bg-gray-300/30"></div>
          <div
            style={{ width: `${percent}%` }}
            className="h-1 rounded-l-full bg-white absolute z-10 inset-0"
          ></div>
        </div>
        <div className="w-full relative flex items-center justify-between">
          <p className="text-xs text-gray-400">
            {fmt(isSeeking ? currentPosition : state.currentTime)}
          </p>
          <p className="text-xs text-gray-400">{fmt(state.duration)}</p>
        </div>
      </div>

      {/* PLAY MODE */}
      <div className="hidden md:inline-flex justify-center items-center gap-5">
        <div className="flex items-center gap-1">
          <div
            onClick={() => handleChangePlayOrderMode("Sequential")}
            className={`p-2 rounded-md flex items-center justify-center 
              transition-all duration-500 ease-out hover:scale-110
              ${
                playOrderMode === "Sequential"
                  ? "bg-white/20 text-white"
                  : "bg-transparent text-gray-400 hover:text-white"
              }`}
          >
            <TbRepeat size={16} />
          </div>
          <div
            onClick={() => handleChangePlayOrderMode("Random")}
            className={`p-2 rounded-md flex items-center justify-center 
              transition-all duration-500 ease-out hover:scale-110
              ${
                playOrderMode === "Random"
                  ? "bg-white/20 text-white"
                  : "bg-transparent text-gray-400 hover:text-white"
              }`}
          >
            <Shuffle size={16} />
          </div>
        </div>

        <div className="flex items-center gap-2 font-poppins">
          <div
            onClick={() => handleChangeIsAutoPlay(!isAutoPlay)}
            className={`p-2 rounded-md bg-white/20 flex items-center justify-center 
              transition-all duration-500 ease-out hover:scale-110
               ${
                 isAutoPlay
                   ? "text-mystic-green"
                   : "text-gray-400 hover:text-white"
               }`}
          >
            <MdAutoMode size={16} />
          </div>
          {isAutoPlay ? (
            <p className="text-xs text-gray-400">
              Auto Play: <span className="font-bold text-mystic-green">On</span>
            </p>
          ) : (
            <p className="text-xs text-gray-400">
              Auto Play: <span className="font-bold">Off</span>
            </p>
          )}
        </div>

        <div className="flex items-center justify-center px-2 gap-3">
          {state.volume === 0 ? (
            <VolumeX color="#D9D9D9" />
          ) : state.volume <= 0.5 ? (
            <Volume1 color="#fff" />
          ) : (
            <Volume2 color="#fff" />
          )}
          <Slider
            defaultValue={[state.volume * 100]}
            onValueChange={(value) => setVolume(value[0] / 100)}
            max={100}
            step={1}
            className="w-24"
          />
        </div>
      </div>
    </div>
  );
};

export default NewMediaPlayerControlBar;
