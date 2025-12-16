// @ts-nocheck

import { IoPause, IoPlay } from "react-icons/io5";
import { IoIosMore } from "react-icons/io";
import { useDispatch, useSelector } from "react-redux";
import {
  playAudio,
  pauseAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import type { RootState } from "@/redux/store";
import PlayingWaveSmall from "@/components/playingWaveSmall/PlayWaveSmall";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { usePlayer } from "@/core/services/player/usePlayer";
import { useNavigate } from "react-router-dom";
import { useLazyCheckUserPodcastListenSlotQuery } from "@/core/services/account/account.service";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";
import { getPlayerController } from "@/core/services/player/playerController";
import type { ListenSessionEpisodes } from "@/core/types/audio";
import { useEffect, useState } from "react";
import { useGetPodcastPublicSourceQuery } from "@/core/services/file/file.service";

type EpisodeCardProps = {
  Episode: {
    Id: string;
    Name: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    AudioLength: number;
    IsReleased: boolean;
  };
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  PodcastEpisodeListenSession: {
    Id: string;
    LastListenDurationSeconds: number;
  };
};

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

const formatAudioLength = (lengthInSeconds: number) => {
  const minutes = Math.floor(lengthInSeconds / 60);
  const seconds = lengthInSeconds % 60;
  return `${minutes}m ${seconds}s`;
};

const calculateProgressPercentage = (
  latestPosition: number,
  audioLength: number
) => {
  if (audioLength === 0) return 0;
  return (latestPosition / audioLength) * 100;
};

const EpisodeCard = ({
  listenSession,
}: {
  listenSession: EpisodeCardProps;
}) => {
  const dispatch = useDispatch();

  const user = useSelector((state: RootState) => state.auth.user);

  // PLAYER CORE
  const {
    play,
    pause,
    playContinueListening,
    state: playerUiState,
  } = usePlayer();

  const controller = getPlayerController();

  // Use state from usePlayer hook for reactive updates
  const state = playerUiState;

  const [fetchBenefits, {}] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();
  const [triggerCheckListenSlot] = useLazyCheckUserPodcastListenSlotQuery();
  const navigate = useNavigate();

  const { data: fileData, isLoading: isLoadingFile } =
    useGetPodcastPublicSourceQuery(
      { FileKey: listenSession.Episode.MainImageFileKey! },
      {
        skip: !listenSession.Episode.MainImageFileKey,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  // RESOLVE FILE URL
  const fileUrl =
    fileData?.FileUrl ||
    "https://i.pinimg.com/736x/1c/c0/8f/1cc08fc01181a676f894534fc73f42cf.jpg";

  // HOOKS
  useEffect(() => {}, []);
  // FUNCTIONS
  const handleContinuePlayEpisode = async (listenSession: EpisodeCardProps) => {
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
        PodcastEpisodeId: listenSession.Episode.Id,
      }).unwrap();
      const benefitList =
        benefitData.CurrentPodcastSubscriptionRegistrationBenefitList;
      if (benefitList && benefitList.length > 0) {
        const hasNonQuota = benefitList.some(
          (s: any) => s?.Id === 1 || s?.Name === "Non-Quota Listening"
        );
        if (hasNonQuota) {
          playContinueListening({
            audioId: listenSession.Episode.Id,
            benefitsList: benefitList,
            seekTo:
              listenSession.PodcastEpisodeListenSession
                .LastListenDurationSeconds,
            continueSessionId: listenSession.PodcastEpisodeListenSession.Id,
          });
        } else {
          // Check listen slots
          const listenSlot = await triggerCheckListenSlot().unwrap();
          if (listenSlot > 0) {
            playContinueListening({
              audioId: listenSession.Episode.Id,
              benefitsList: benefitList,
              seekTo:
                listenSession.PodcastEpisodeListenSession
                  .LastListenDurationSeconds,
              continueSessionId: listenSession.PodcastEpisodeListenSession.Id,
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
          playContinueListening({
            audioId: listenSession.Episode.Id,
            benefitsList: benefitList,
            seekTo:
              listenSession.PodcastEpisodeListenSession
                .LastListenDurationSeconds,
            continueSessionId: listenSession.PodcastEpisodeListenSession.Id,
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
      if (
        state.currentAudio &&
        state.currentAudio.id === listenSession.Episode.Id
      ) {
        play();
      } else {
        await handleContinuePlayEpisode(listenSession);
      }
    }
  };

  return (
    <div
      style={{
        backgroundImage: `url(${fileUrl})`,
      }}
      className="bg-cover w-full aspect-[3/4] rounded-xl relative transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer"
    >
      <div className="w-full aspect-square">
        <AutoResolveImage
          FileKey={listenSession.Episode.MainImageFileKey}
          type="PodcastPublicSource"
          Name={listenSession.Episode.Name || "episode-image"}
          imgClassName="w-full aspect-square object-cover rounded-t-xl"
        />
      </div>

      <div
        className="
            rounded-b-xl
            pointer-events-none absolute inset-0
            backdrop-blur-[200px] backdrop-saturate-200
            [mask-image:linear-gradient(to_top,black_30%,transparent_100%)]
            [mask-size:cover]
        "
      />

      <div
        className="
            rounded-xl
            pointer-events-none absolute inset-0
            bg-gradient-to-t from-black/50 via-transparent/30 to-transparent
            [mask-image:linear-gradient(to_top,black_70%,transparent_100%)]
        "
      />

      <div className="absolute w-full bottom-14 z-10 p-5 flex flex-col items-start justify-between gap-3">
        <div className="w-full flex items-center justify-start">
          <AutoResolveImage
            FileKey={listenSession.Podcaster.MainImageFileKey}
            type="PodcastPublicSource"
            Name={listenSession.Podcaster.FullName || "podcaster-image"}
            className="w-10 h-10 flex items-center justify-center"
            imgClassName="w-10 h-10 rounded-xl aspect-square object-cover shadow-sm"
          />
        </div>

        <div className="w-full">
          <p className="text-gray-300 text-xs">
            {getTimeRange(listenSession.Episode.ReleaseDate)} AGO
          </p>
          <p className="text-white font-bold text-xl line-clamp-1">
            {listenSession.Episode.Name}
          </p>
        </div>
      </div>
      <div className="absolute bottom-0 z-20 bg-black/40 backdrop-blur-md right-0 left-0 rounded-b-md">
        <div className="w-full flex items-center justify-start relative">
          <div className="w-full flex items-center gap-2 justify-between p-5">
            <div
              onClick={() => handlePlayPause()}
              className="w-full flex items-center justify-center gap-1 py-1 px-2 bg-white rounded-xl cursor-pointer hover:bg-gray-100 transition-colors"
            >
              {state.isPlaying &&
              state.currentAudio &&
              state.currentAudio.id === listenSession.Episode.Id ? (
                <div className="z-20 relative w-5 h-5 overflow-hidden flex items-center justify-center">
                  <IoPause className="h-5 w-5 text-black" />
                </div>
              ) : (
                <IoPlay className="h-5 w-5 text-black" />
              )}
              <p className="font-poppins m-0 text-xs font-semibold text-[#333]">
                {state.isPlaying &&
                state.currentAudio &&
                state.currentAudio.id === listenSession.Episode.Id
                  ? "Pause"
                  : `Continue`}
              </p>
            </div>
          </div>

          {state.currentAudio &&
          state.currentAudio.id === listenSession.Episode.Id ? (
            <div
              style={{
                width: `${calculateProgressPercentage(
                  state.currentTime,
                  listenSession.Episode.AudioLength
                )}%`,
              }}
              className="absolute top-0 h-0.5 bg-mystic-green"
            />
          ) : (
            <div
              style={{
                width: `${calculateProgressPercentage(
                  listenSession.PodcastEpisodeListenSession
                    .LastListenDurationSeconds,
                  listenSession.Episode.AudioLength
                )}%`,
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
