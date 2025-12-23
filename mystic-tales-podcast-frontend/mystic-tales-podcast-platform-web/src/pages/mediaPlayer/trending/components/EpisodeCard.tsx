import { IoPause, IoPlay } from "react-icons/io5";
import { useDispatch, useSelector } from "react-redux";

import type { RootState } from "@/redux/store";

import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { usePlayer } from "@/core/services/player/usePlayer";
import { useNavigate } from "react-router-dom";
import { useLazyCheckUserPodcastListenSlotQuery } from "@/core/services/account/account.service";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";
import { useGetPodcastPublicSourceQuery } from "@/core/services/file/file.service";

import type { EpisodeFromTrending } from "@/core/types/feed";
import ContentFallBackImage from "/images/unknown/content.png";
import ActivityIndicator from "@/components/loader/ActivityIndicator";
import { debouncePromise } from "@/core/utils/debouncePromise";

const getTimeRange = (releaseDate: string) => {
  const now = new Date();
  const release = new Date(releaseDate);
  const diffMs = Math.abs(now.getTime() - release.getTime());
  const diffHours = diffMs / (1000 * 60 * 60);

  if (diffHours >= 24) {
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays}D`;
  } else {
    const rounded = Math.floor(diffHours);
    return `${rounded}H`;
  }
};

// const formatAudioLength = (lengthInSeconds: number) => {
//   const minutes = Math.floor(lengthInSeconds / 60);
//   const seconds = lengthInSeconds % 60;
//   return `${minutes}m ${seconds}s`;
// };

const calculateProgressPercentage = (
  latestPosition: number,
  audioLength: number
) => {
  if (audioLength === 0) return 0;
  return (latestPosition / audioLength) * 100;
};

const EpisodeCard = ({ episode }: { episode: EpisodeFromTrending }) => {
  const dispatch = useDispatch();

  const user = useSelector((state: RootState) => state.auth.user);

  // PLAYER CORE
  const {
    play,
    pause,
    playEpisodeFromSpecifyShow,
    state: playerUiState,
  } = usePlayer();

  // const controller = getPlayerController();

  // Use state from usePlayer hook for reactive updates
  const state = playerUiState;

  const [fetchBenefits, {}] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();
  const [triggerCheckListenSlot] = useLazyCheckUserPodcastListenSlotQuery();
  const navigate = useNavigate();

  const { data: fileData } = useGetPodcastPublicSourceQuery(
    { FileKey: episode.MainImageFileKey! },
    {
      skip: !episode.MainImageFileKey,
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    }
  );

  // RESOLVE FILE URL
  const fileUrl = fileData?.FileUrl || ContentFallBackImage;

  // FUNCTIONS
  const handlePlayEpisodeFromSpecifyShow = async (episodeId: string) => {
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
      const benefitData = await fetchBenefits({
        PodcastEpisodeId: episodeId,
      }).unwrap();
      const benefitList =
        benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;
      if (benefitList && benefitList.length > 0) {
        const hasNonQuota = benefitList.some(
          (s: any) => s?.Id === 1 || s?.Name === "Non-Quota Listening"
        );
        if (hasNonQuota) {
          playEpisodeFromSpecifyShow({
            audioId: episodeId,
            benefitsList: benefitList,
          });
        } else {
          // Check listen slots
          const listenSlot = await triggerCheckListenSlot().unwrap();
          if (listenSlot > 0) {
            playEpisodeFromSpecifyShow({
              audioId: episodeId,
              benefitsList: benefitList,
            });
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
            audioId: episodeId,
            benefitsList: benefitList,
          });
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
  };

  const handlePlayPause = async () => {
    if (state.isPlaying) {
      pause();
    } else {
      if (state.currentAudio && state.currentAudio.id === episode.Id) {
        play();
      } else {
        await handlePlayEpisodeFromSpecifyShow(episode.Id);
      }
    }
  };
  const debouncePlay = debouncePromise(handlePlayPause, 1000);

  return (
    <div
      style={{
        backgroundImage: `url(${fileUrl})`,
      }}
      className="bg-cover w-full aspect-3/4 rounded-xl relative transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer"
    >
      <div className="w-full aspect-square">
        <AutoResolveImage
          FileKey={episode.MainImageFileKey}
          type="PodcastPublicSource"
          Name={episode.Name || "episode-image"}
          imgClassName="w-full aspect-square object-cover rounded-t-xl"
        />
      </div>

      <div
        className="
            rounded-b-xl
            pointer-events-none absolute inset-0
            backdrop-blur-[200px] backdrop-saturate-200
            mask-[linear-gradient(to_top,black_30%,transparent_100%)]
            mask-cover
        "
      />

      <div
        className="
            rounded-xl
            pointer-events-none absolute inset-0
            bg-linear-to-t from-black/50 via-transparent/30 to-transparent
            mask-[linear-gradient(to_top,black_70%,transparent_100%)]
        "
      />

      <div className="absolute w-full bottom-14 z-10 p-5 flex flex-col items-start justify-between gap-3">
        <div className="w-full">
          <p className="text-gray-300 text-xs">
            {getTimeRange(episode.ReleaseDate)} AGO
          </p>
          <p className="text-white font-bold text-xl line-clamp-1">
            {episode.Name}
          </p>
        </div>
      </div>
      <div className="absolute bottom-0 z-20 backdrop-blur-md right-0 left-0 rounded-b-md">
        <div className="w-full flex items-center justify-start relative">
          <div className="w-full flex items-center gap-2 justify-between p-5">
            {state.isLoadingSession && state.loadingAudioId === episode.Id ? (
              <div className="w-full flex items-center justify-center gap-1 py-1 px-2 bg-white rounded-xl cursor-pointer hover:bg-gray-100 transition-colors">
                <div className="z-20 relative w-5 h-5 overflow-hidden flex items-center justify-center">
                  <ActivityIndicator size={20} color="#000" />
                </div>
                <p className="font-poppins m-0 text-xs font-semibold text-[#333]">
                  Loading
                </p>
              </div>
            ) : (
              <div
                onClick={() => debouncePlay()}
                className={`w-full flex items-center justify-center gap-1 py-1 px-2 bg-white rounded-xl hover:bg-gray-100 transition-colors ${
                  state.isLoadingSession
                    ? "cursor-not-allowed"
                    : "cursor-pointer"
                }`}
              >
                {state.isPlaying &&
                state.currentAudio &&
                state.currentAudio.id === episode.Id ? (
                  <div className="z-20 relative w-5 h-5 overflow-hidden flex items-center justify-center">
                    <IoPause className="h-5 w-5 text-black" />
                  </div>
                ) : (
                  <IoPlay className="h-5 w-5 text-black" />
                )}
                <p className="font-poppins m-0 text-xs font-semibold text-[#333]">
                  {state.isPlaying &&
                  state.currentAudio &&
                  state.currentAudio.id === episode.Id
                    ? "Pause"
                    : `Continue`}
                </p>
              </div>
            )}
          </div>
          {state.currentAudio && state.currentAudio.id === episode.Id ? (
            <div
              style={{
                width: `${calculateProgressPercentage(
                  state.currentTime,
                  episode.AudioLength
                )}%`,
              }}
              className="absolute top-0 h-0.5 bg-mystic-green"
            />
          ) : (
            <div
              style={{
                width: `0%`,
              }}
              className="absolute top-0 h-0.5 bg-mystic-green"
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default EpisodeCard;
