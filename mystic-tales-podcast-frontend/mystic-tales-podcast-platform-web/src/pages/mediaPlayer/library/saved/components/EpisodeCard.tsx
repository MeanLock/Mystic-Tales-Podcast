import { IoPause, IoPlay } from "react-icons/io5";
import { IoIosMore } from "react-icons/io";
import type { EpisodeUI } from "@/core/types/episode";
import { useNavigate } from "react-router-dom";
import { renderDescriptionHTML } from "@/pages/mediaPlayer/channels/details";
import { useDispatch } from "react-redux";
import { playAudio } from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { usePlayer } from "@/core/services/player/usePlayer";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { Tag } from "lucide-react";
import { BsFillBookmarkFill } from "react-icons/bs";
import { useSaveEpisodeMutation } from "@/core/services/episode/episode.service";

const EpisodeCard = ({
  episode,
  handleUnSaveEpisode,
}: {
  episode: EpisodeUI;
  handleUnSaveEpisode: (podcastEpisodeId: string) => void;
}) => {
  const {
    playEpisodeFromSavedEpisodes,
    play,
    pause,
    state: uiState,
  } = usePlayer();

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

  const formatAudioLength = (audioLength: number) => {
    const hours = Math.floor(audioLength / 3600);
    const minutes = Math.floor((audioLength % 3600) / 60);
    const seconds = audioLength % 60;

    if (hours > 0) {
      return `${hours}h ${minutes}m ${seconds}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${seconds}s`;
    } else {
      return `${seconds}s`;
    }
  };

  const dispatch = useDispatch();
  const navigate = useNavigate();

  const [triggerGetBenefitList] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();

  const handlePlayPauseSavedEpisodes = async () => {
    if (
      uiState.isPlaying &&
      uiState.currentAudio &&
      uiState.currentAudio.id === episode.Id
    ) {
      pause();
      return;
    } else if (
      !uiState.isPlaying &&
      uiState.currentAudio &&
      uiState.currentAudio.id === episode.Id
    ) {
      play();
      return;
    } else {
      const benefitsList = await triggerGetBenefitList({
        PodcastEpisodeId: episode.Id,
      }).unwrap();
      playEpisodeFromSavedEpisodes({
        audioId: episode.Id,
        benefitsList:
          benefitsList.CurrentPodcastSubscriptionRegistrationBenefitList,
      });
    }
  };

  return (
    <div
      style={{ backgroundImage: `url(${episode.ImageUrl})` }}
      className="bg-cover w-full aspect-[3/4] rounded-xl relative transition-all duration-500 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer"
    >
      <div className="w-full aspect-square">
        <img
          src={episode.ImageUrl}
          className="w-full aspect-square object-cover rounded-t-xl"
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

      <div className="absolute w-full bottom-0 z-10 p-5 flex flex-col items-start justify-between gap-3">
        <div className="w-full flex items-center justify-start">
          <img
            src={episode.PodcastShow.ImageUrl}
            className="w-10 h-10 rounded-xl aspect-square object-cover shadow-sm"
          />
        </div>

        <div className="w-full">
          <p className="text-gray-300 text-xs">
            {getTimeRange(episode.ReleaseDate)} AGO
          </p>
          <p className="text-white font-bold text-xl line-clamp-1">
            {episode.Name}
          </p>
          {/* <p className="text-gray-100 line-clamp-1 text-xs">
            {episode.Description}
          </p> */}
          <div
            className="text-gray-100 line-clamp-3 text-xs"
            dangerouslySetInnerHTML={{
              __html: renderDescriptionHTML(episode.Description),
            }}
          />
        </div>
        <div className="w-full flex items-center justify-between">
          <div
            onClick={(e) => {
              e.stopPropagation();
              handlePlayPauseSavedEpisodes();
            }}
            className="px-5 py-1 gap-1 bg-white rounded-xl flex items-center justify-center"
          >
            {uiState.isPlaying &&
            uiState.currentAudio &&
            uiState.currentAudio.id === episode.Id ? (
              <IoPause size={15} color="#333" />
            ) : (
              <IoPlay size={15} color="#333" />
            )}
            <p className="font-poppins m-0 text-sm font-semibold text-[#333]">
              {formatAudioLength(episode.AudioLength)}
            </p>
          </div>
          <div
            onClick={() => handleUnSaveEpisode(episode.Id)}
            className="flex p-2 rounded-full items-center justify-center text-white bg-gray-300/30 hover:bg-gray-300/50"
          >
            <BsFillBookmarkFill color="#aee339" size={16} />
          </div>
        </div>
      </div>
    </div>
  );
};

export default EpisodeCard;

{
  /* <div
        className="
          pointer-events-none absolute inset-0
          backdrop-blur-[14px] backdrop-saturate-150
          [mask-image:linear-gradient(to_top,black_52%,transparent_75%)]
          "
      /> */
}
