import Loading from "@/components/loading";
import {
  useGetEpisodeDetailsQuery,
  useSaveEpisodeMutation,
} from "@/core/services/episode/episode.service";
import { IoArrowBack, IoPause, IoPlay } from "react-icons/io5";
import { useNavigate, useParams } from "react-router-dom";

import { BsFillBookmarkFill } from "react-icons/bs";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import {
  pauseAudio,
  playAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useLazyCheckUserPodcastListenSlotQuery } from "@/core/services/account/account.service";
import { useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery } from "@/core/services/subscription/subscription.service";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";


// Helper function to format duration
const formatDuration = (seconds: number): string => {
  const minutes = Math.floor(seconds / 60);
  if (minutes < 1) {
    return `${seconds} sec`;
  }
  return `${minutes} min`;
};

// Helper function to format time ago
const getTimeAgo = (dateString: string): string => {
  const date = new Date(dateString);
  const now = new Date();
  const diffInDays = Math.floor(
    (now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24)
  );

  if (diffInDays === 0) return "Today";
  if (diffInDays === 1) return "1 day ago";
  return `${diffInDays} days ago`;
};

export function renderDescriptionHTML(description: string | null) {
  if (!description) return "";

  // --- Tách link ---
  const linkRegex = /\$-\[link\]\$-([\s\S]*?)\$-\[link\]\$-/;
  const linkMatch = description.match(linkRegex);
  const link = linkMatch ? linkMatch[1].trim() : null;

  // --- Tách script ---
  const scriptRegex = /\$-\[script\]\$-([\s\S]*?)\$-\[script\]\$-/;
  const scriptMatch = description.match(scriptRegex);
  const scriptContent = scriptMatch ? scriptMatch[1].trim() : null;

  // --- Loại bỏ các phần đặc biệt khỏi phần mô tả còn lại ---
  let cleanDescription = description
    .replace(linkRegex, "")
    .replace(scriptRegex, "")
    .trim();

  // --- Tạo HTML ---
  let html = `<p>${cleanDescription}</p>`;

  if (link) {
    html += `
    <p><strong>Link</strong>: <a href="${link}" target="_blank" rel="noopener noreferrer">${link}</a></p>`;
  }

  if (scriptContent) {
    html += `
    <p><strong>Script</strong>:</p>
    <div style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px; background-color: #f9f9f9;">
      ${scriptContent}
    </div>
    `;
  }

  return html.trim();
}

const EpisodeDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  // STATES
  const [IsSavedByCurrentUser, setIsSavedByCurrentUser] = useState(false);

  // HOOKS,
  const player = useSelector((state: RootState) => state.player);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const {
    data: episodeDetailsRaw,
    isLoading: isLoadingEpisodeDetails,
    refetch: refetchEpisodeData,
  } = useGetEpisodeDetailsQuery(
    { PodcastEpisodeId: id! },
    {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
      skip: !id,
    }
  );

  useEffect(() => {
    if (episodeDetailsRaw) {
      setIsSavedByCurrentUser(episodeDetailsRaw.Episode.IsSavedByCurrentUser);
    }
  }, [episodeDetailsRaw]);

  const [toggleSaveEpisode] = useSaveEpisodeMutation();

  // Lazy queries: only trigger inside play handler
  const [checkListenSlotTrigger] = useLazyCheckUserPodcastListenSlotQuery();
  const [getBenefitsTrigger] =
    useLazyGetSubscriptionBenefitsMapListFromEpisodeIdQuery();

  // FUNCTIONS
  const handleToggleSaveEpisode = async (shouldSave: boolean) => {
    const restoreValue = IsSavedByCurrentUser;
    // Gọi API để lưu hoặc bỏ lưu episode
    setIsSavedByCurrentUser(shouldSave);
    if (!episodeDetailsRaw) return;
    try {
      await toggleSaveEpisode({
        PodcastEpisodeId: episodeDetailsRaw.Episode.Id,
        IsSave: shouldSave,
      }).unwrap();
      // Refetch dữ liệu chi tiết episode để cập nhật lại trạng thái
      await refetchEpisodeData();
    } catch (error) {
      setIsSavedByCurrentUser(restoreValue);
    }
  };

  const handlePlayEpisodeFromShow = async () => {
    if (!episodeDetailsRaw?.Episode?.Id) return;
    try {
      // Trigger lazy queries when Play is clicked
      const listenSlotResult = await checkListenSlotTrigger().unwrap();
      const benefitsResult = await getBenefitsTrigger({
        PodcastEpisodeId: episodeDetailsRaw.Episode.Id,
      }).unwrap();

      if (
        benefitsResult.CurrentPodcastSubscriptionRegistrationBenefitList
          .length > 0
      ) {
        const benefitList =
          benefitsResult.CurrentPodcastSubscriptionRegistrationBenefitList;
        const hasListenBenefit = benefitList.some(
          (benefit) => benefit.Id === 1
        );
        if (hasListenBenefit) {
          dispatch(
            playAudio({
              audioId: episodeDetailsRaw.Episode.Id,
              sourceType: "SpecifyShowEpisodes",
            })
          );
        } else {
          if (listenSlotResult > 0) {
            dispatch(
              playAudio({
                audioId: episodeDetailsRaw.Episode.Id,
                sourceType: "SpecifyShowEpisodes",
              })
            );
          } else {
            alert(
              "You have no remaining podcast listen slots. Please subscribe to a podcast plan to continue listening."
            );
          }
        }
      } else {
        if (listenSlotResult > 0) {
          dispatch(
            playAudio({
              audioId: episodeDetailsRaw.Episode.Id,
              sourceType: "SpecifyShowEpisodes",
            })
          );
        } else {
          alert(
            "You have no remaining podcast listen slots. Please subscribe to a podcast plan to continue listening."
          );
        }
      }
    } catch (err) {
      // Optionally surface error via toast or redux error slice
      // console.error("Failed to check play entitlement", err);
    }
  };

  if (isLoadingEpisodeDetails) {
    return (
      <div className="w-full h-full flex items-center justify-center flex-col gap-5">
        <Loading />
        <p className="text-[#D9D9D9] font-poppins font-bold">
          Loading episode details...
        </p>
      </div>
    );
  }

  if (!episodeDetailsRaw && !isLoadingEpisodeDetails || !episodeDetailsRaw?.Episode) {
    return (
      <div className="w-full h-full flex items-center justify-center flex-col gap-5">
        <p className="text-red-400 font-poppins font-light">
          Episode not found.
        </p>
        <LiquidButton
          variant="danger"
          onClick={() => navigate("/media-player/discovery")}
        >
          Go Back
        </LiquidButton>
      </div>
    );
  } else {
    return (
      <div className="w-full h-full flex flex-col">
        <div className="w-full flex items-center p-8">
          <div
            className="flex items-center text-white gap-2 hover:underline cursor-pointer"
            onClick={() => navigate(-1)}
          >
            <IoArrowBack />
            <p className="font-poppins">Back</p>
          </div>
        </div>
        <div className="w-full h-[400px] flex items-center p-10 relative">
          <div className="h-full aspect-square rounded-md flex items-center justify-center">
            <AutoResolveImage
              FileKey={episodeDetailsRaw.Episode.MainImageFileKey}
              type="PodcastPublicSource"
              className="w-full h-full rounded-md object-cover shadow-2xl"
            />
          </div>
          <div className="ml-10 h-full flex flex-col items-start justify-center gap-2">
            <p className="text-[#D9D9D9] uppercase text-md leading-none">
              {getTimeAgo(episodeDetailsRaw?.Episode.ReleaseDate || "")} -{" "}
              {formatDuration(episodeDetailsRaw?.Episode.AudioLength || 0)}
            </p>
            <p className="text-white text-7xl line-clamp-1 font-bold leading-none">
              {episodeDetailsRaw?.Episode.Name}
            </p>
            <p className="text-mystic-green uppercase text-2xl leading-none">
              {episodeDetailsRaw?.Episode.Podcaster.FullName}
            </p>
            {/* Play button */}
            {player.playMode.playStatus === "play" &&
            player.currentAudio?.Id === episodeDetailsRaw?.Episode.Id ? (
              <div
                onClick={() => dispatch(pauseAudio())}
                className="mt-5 px-8 py-2 bg-mystic-green font-poppins font-semibold text-black rounded-full flex items-center justify-center gap-2 cursor-pointer shadow-lg transition-all duration-500 hover:scale-105 hover:shadow-sm"
              >
                <IoPause size={20} color="#000" />
                <p>Pause</p>
              </div>
            ) : (
              <div
                onClick={() => handlePlayEpisodeFromShow()}
                className="mt-5 px-8 py-2 bg-mystic-green font-poppins font-semibold text-black rounded-full flex items-center justify-center gap-2 cursor-pointer shadow-lg transition-all duration-500 hover:scale-105 hover:shadow-sm"
              >
                <IoPlay size={20} color="#000" />
                <p>Play</p>
              </div>
            )}
          </div>
          <div
            onClick={() => handleToggleSaveEpisode(!IsSavedByCurrentUser)}
            className="absolute right-12 bottom-12 rounded-full p-2 bg-white/20 transition-all duration-500 hover:scale-110 cursor-pointer"
          >
            {IsSavedByCurrentUser ? (
              <BsFillBookmarkFill color="#aee339" size={12} />
            ) : (
              <BsFillBookmarkFill color="#fff" size={12} />
            )}
          </div>
        </div>
        <div className="w-full mt-10 px-8">
          <div
            className="prose prose-invert max-w-full text-white line-clamp-3"
            dangerouslySetInnerHTML={{
              __html: renderDescriptionHTML(
                episodeDetailsRaw?.Episode.Description || null
              ),
            }}
          />
        </div>

        <div className="w-full mt-10 px-8 flex flex-col gap-3">
          <p className="text-white font-poppins text-3xl font-bold mb-2">
            Episode Informations
          </p>
          <div className="w-full grid grid-cols-2 md:grid-cols-4">
            <div className="flex flex-col gap-1 font-poppins">
              <p className="text-[#D9D9D9] font-semibold">Show</p>
              <p
                onClick={() =>
                  navigate(
                    `/media-player/shows/${episodeDetailsRaw?.Episode.PodcastShow.Id}`
                  )
                }
                className="text-mystic-green font-light hover:underline italic cursor-pointer"
              >
                {episodeDetailsRaw?.Episode.PodcastShow.Name}
              </p>
            </div>

            <div className="flex flex-col gap-1 font-poppins">
              <p className="text-[#D9D9D9] font-semibold">Podcaster</p>
              <p
                onClick={() =>
                  navigate(
                    `/media-player/podcasters/${episodeDetailsRaw?.Episode.Podcaster.Id}`
                  )
                }
                className="text-mystic-green font-light hover:underline italic cursor-pointer"
              >
                {episodeDetailsRaw?.Episode.Podcaster.FullName}
              </p>
            </div>
          </div>
        </div>
      </div>
    );
  }
};

export default EpisodeDetailsPage;
