import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import ActivityIndicator from "@/components/loader/ActivityIndicator";
import PlayingWave from "@/components/playingWave/PlayWave";
import { useLazyCheckUserPodcastListenSlotQuery } from "@/core/services/account/account.service";
import type { ListenHistory } from "@/core/services/episode/episode.service";
import { usePlayer } from "@/core/services/player/usePlayer";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { debouncePromise } from "@/core/utils/debouncePromise";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";
import type { RootState } from "@/redux/store";
import { IoPlay } from "react-icons/io5";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";

const formatAudioLength = (lengthInSeconds: number): string => {
  const minutes = Math.floor(lengthInSeconds / 60);
  const seconds = lengthInSeconds % 60;
  if (minutes < 10) {
    return `0${minutes}:${seconds.toString().padStart(2, "0")}`;
  }
  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
};

const HistoryRow = ({
  history,
  onListen,
}: {
  history: ListenHistory;
  onListen: () => void;
}) => {
  const {
    state: uiState,
    play,
    pause,
    playEpisodeFromSpecifyShow,
  } = usePlayer();

  const dispatch = useDispatch();
  const navigate = useNavigate();
  const user = useSelector((state: RootState) => state.auth.user);

  const [fetchBenefits] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();
  const [triggerCheckListenSlot] = useLazyCheckUserPodcastListenSlotQuery();

  const handlePlayPauseEpisode = async () => {
    if (!user) {
      dispatch(
        showAlert({
          title: "Login Required",
          description: "You need to login first to play an episode!",
          type: "warning",
          isAutoClose: false,
          isFunctional: true,
          isClosable: true,
          functionalButtonText: "Login Now",
          onClickAction: () => {
            navigate("/auth/login");
          },
        })
      );
    } else {
      // Check benefit registrations
      if (
        uiState.isPlaying &&
        uiState.currentAudio &&
        uiState.currentAudio.id === history.PodcastEpisode.Id
      ) {
        pause();
        return;
      } else if (
        !uiState.isPlaying &&
        uiState.currentAudio &&
        uiState.currentAudio.id === history.PodcastEpisode.Id
      ) {
        play();
        return;
      } else {
        const benefitData = await fetchBenefits({
          PodcastEpisodeId: history.PodcastEpisode.Id,
        }).unwrap();
        const benefitList =
          benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;
        if (benefitList && benefitList.length > 0) {
          const hasNonQuota = benefitList.some(
            (s: any) => s?.Id === 1 || s?.Name === "Non-Quota Listening"
          );
          if (hasNonQuota) {
            playEpisodeFromSpecifyShow({
              audioId: history.PodcastEpisode.Id,
              benefitsList: benefitList,
            });
            onListen();
          } else {
            // Check listen slots
            const listenSlot = await triggerCheckListenSlot().unwrap();
            if (listenSlot > 0) {
              playEpisodeFromSpecifyShow({
                audioId: history.PodcastEpisode.Id,
                benefitsList: benefitList,
              });
              onListen();
            } else {
              dispatch(
                showAlert({
                  title: "No Listen Slots Left",
                  description:
                    "You have no remaining podcast listen slots. Please wait for your slots to renew.",
                  type: "error",
                  isAutoClose: true,
                  autoCloseDuration: 10,
                  isClosable: true,
                })
              );
            }
          }
        } else {
          // No benefits, check listen slots
          const listenSlot = await triggerCheckListenSlot().unwrap();
          if (listenSlot > 0) {
            playEpisodeFromSpecifyShow({
              audioId: history.PodcastEpisode.Id,
              benefitsList: [],
            });
            onListen();
          } else {
            dispatch(
              showAlert({
                title: "No Listen Slots Left",
                description:
                  "You have no remaining podcast listen slots. Please wait for your slots to renew.",
                type: "error",
                isAutoClose: true,
                autoCloseDuration: 10,
                isClosable: true,
              })
            );
          }
        }
      }
    }
  };

  const debouncePlay = debouncePromise(handlePlayPauseEpisode, 300);

  return (
    <div
      key={history.PodcastEpisode.Id + history.CreatedAt}
      className="bg-white/10 p-4 rounded-lg hover:bg-white/20 transition-all flex items-center gap-5"
    >
      <div className="w-16 h-16 group flex items-center justify-center relative">
        <AutoResolveImage
          FileKey={history.PodcastEpisode.MainImageFileKey}
          type="PodcastPublicSource"
          className="w-16 h-16 rounded-lg"
        />
        {uiState.isLoadingSession &&
        uiState.loadingAudioId === history.PodcastEpisode.Id ? (
          <div
            className={`absolute inset-0 flex bg-black/30 items-center justify-center rounded-lg cursor-not-allowed`}
          >
            <div className="p-2 rounded-full flex items-center justify-center ">
              <ActivityIndicator size={25} color="#fff" />
            </div>
          </div>
        ) : (
          <div
            onClick={() => debouncePlay()}
            className={`absolute inset-0 flex bg-black/30 items-center justify-center rounded-lg ${
              uiState.isPlaying &&
              uiState.currentAudio &&
              uiState.currentAudio.id === history.PodcastEpisode.Id
                ? ""
                : "hidden group-hover:flex"
            }
          ${uiState.isLoadingSession ? "cursor-not-allowed" : "cursor-pointer"}
          `}
          >
            <div className="p-2 rounded-full bg-mystic-green flex items-center justify-center ">
              {uiState.isPlaying &&
              uiState.currentAudio &&
              uiState.currentAudio.id === history.PodcastEpisode.Id ? (
                <PlayingWave />
              ) : (
                <IoPlay size={20} color="#ffffff" />
              )}
            </div>
          </div>
        )}
      </div>
      <div className="flex flex-col items-start justify-start w-125">
        <p className="text-white font-semibold">
          {history.PodcastEpisode.Name}
        </p>
        <div
          className="text-[#d9d9d9] font-light line-clamp-2 text-sm"
          dangerouslySetInnerHTML={{
            __html: history.PodcastEpisode.Description,
          }}
        />
      </div>
      <div className="w-75 flex items-center justify-center">
        <p className="text-white font-semibold text-sm">
          {formatAudioLength(history.PodcastEpisode.AudioLength)}
        </p>
      </div>
      <div className="flex-1 h-full relative">
        <p className="text-[#d9d9d9] absolute top-0 right-0 text-sm">
          {new Date(history.CreatedAt).toLocaleTimeString("en-US", {
            hour: "2-digit",
            minute: "2-digit",
          })}
        </p>
      </div>
    </div>
  );
};

export default HistoryRow;
