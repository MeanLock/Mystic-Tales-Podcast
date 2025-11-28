import { IoPause, IoPlay } from "react-icons/io5";
import { IoIosMore } from "react-icons/io";
import { useDispatch, useSelector } from "react-redux";
import {
  playAudio,
  pauseAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import type { RootState } from "@/redux/store";
import PlayingWaveSmall from "@/components/playingWaveSmall/PlayWaveSmall";

type EpisodeCardProps = {
  Episode: {
    Id: string;
    Name: string;
    ImageUrl: string;
    ReleaseDate: string;
    AudioLength: number;
    IsReleased: boolean;
  };
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    ImageUrl: string;
  };
  PodcastEpisodeListenSession: {
    Id: string;
    LastListenDurationSeconds: number;
  };
};

const EpisodeCard = ({
  listenSession,
}: {
  listenSession: EpisodeCardProps;
}) => {
  const dispatch = useDispatch();
  const player = useSelector((state: RootState) => state.player);

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

  const handlePlayPause = () => {
    const isCurrentEpisode =
      player.currentAudio?.Id === listenSession.Episode.Id;
    const isPlaying = player.playMode.playStatus === "play";

    if (isCurrentEpisode && isPlaying) {
      // Nếu đang phát episode này, thì pause
      dispatch(pauseAudio());
    } else if (isCurrentEpisode && !isPlaying) {
      // Nếu là episode này nhưng đang pause/stop, thì resume
      dispatch(
        playAudio({
          audioId: listenSession.Episode.Id,
          sourceType: "SpecifyShowEpisodes",
        })
      );
    } else {
      // Nếu là episode khác, thì continue playing từ vị trí đã lưu
      dispatch(
        playAudio({
          audioId: listenSession.Episode.Id,
          sourceType: "SpecifyShowEpisodes",
          continue_listen_session_id:
            listenSession.PodcastEpisodeListenSession.Id,
          seekTo:
            listenSession.PodcastEpisodeListenSession.LastListenDurationSeconds,
        })
      );
    }
  };

  const isCurrentEpisode = player.currentAudio?.Id === listenSession.Episode.Id;
  const isPlaying = player.playMode.playStatus === "play";

  return (
    <div
      // onClick={() =>
      //   navigate(`/media-player/episodes/${listenSession.Episode.Id}`)
      // }
      style={{ backgroundImage: `url(${listenSession.Episode.ImageUrl})` }}
      className="bg-cover w-full aspect-[3/4] rounded-xl relative transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer"
    >
      <div className="w-full aspect-square">
        <img
          src={listenSession.Episode.ImageUrl}
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
            src={listenSession.Podcaster.ImageUrl}
            className="w-10 h-10 rounded-xl aspect-square object-cover shadow-sm"
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
        <div className="w-full flex items-center  gap-2 justify-between">
          <div
            onClick={handlePlayPause}
            className="gap-1 py-1 px-2 bg-white rounded-xl flex items-center justify-start cursor-pointer hover:bg-gray-100 transition-colors"
          >
            {isCurrentEpisode && isPlaying ? (
              <div className="z-20 relative w-5 h-5 overflow-hidden flex items-center justify-center">
                <IoPause className="h-5 w-5 text-black" />
              </div>
            ) : (
              <IoPlay className="h-5 w-5 text-black" />
            )}
            <p className="font-poppins m-0 text-xs font-semibold text-[#333]">
              {isCurrentEpisode && isPlaying ? "Pause" : "Continue Playing"}
            </p>
          </div>
          <div className="flex p-1 rounded-full bg-transparent items-center justify-center text-white hover:bg-gray-300/30">
            <IoIosMore size={20} />
          </div>
        </div>
      </div>
    </div>
  );
};

export default EpisodeCard;
