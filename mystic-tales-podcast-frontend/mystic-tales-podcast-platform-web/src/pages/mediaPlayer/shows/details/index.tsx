import Loading from "@/components/loading";
import { Button } from "@/components/ui/button";
import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogDescription,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Check } from "lucide-react";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";
import {
  useFollowShowMutation,
  useGetActiveShowSubscriptionQuery,
  useGetShowDetailsQuery,
  useRatingShowMutation,
  useUnFollowShowMutation,
} from "@/core/services/show/show.service";
import {
  useGetCustomerRegistrationInfoFromShowQuery,
  useSubscribePodcastSubscriptionMutation,
  useUnsubscribePodcastSubscriptionMutation,
} from "@/core/services/subscription/subscription.service";
import type { ShowDetailsUI, ShowUI } from "@/core/types/show";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import type { RootState } from "@/redux/store";
import { useEffect, useState } from "react";
import { FaPlus } from "react-icons/fa6";
import { FaPlay } from "react-icons/fa6";
import { IoHeartOutline, IoHeartSharp, IoPause, IoPlay } from "react-icons/io5";
import { LiaDizzy } from "react-icons/lia";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate, useParams } from "react-router-dom";
import {
  pauseAudio,
  playAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useUpdatePlayModeMutation } from "@/core/services/player/player.service";
import { set } from "lodash";
import PlayingWave from "@/components/playingWave/PlayWave";

const ShowFileConfig: FileResolveConfig[] = [
  {
    path: "Show.MainImageFileKey",
    output: "Show.ImageUrl",
    type: "PodcastPublic",
  },
  {
    path: "Show.Podcaster.MainImageFileKey",
    output: "Show.Podcaster.ImageUrl",
    type: "AccountPublic",
  },
  {
    path: "Show.PodcastChannel.MainImageFileKey",
    output: "Show.PodcastChannel.ImageUrl",
    type: "PodcastPublic",
  },
  {
    path: "Show.EpisodeList[].MainImageFileKey",
    output: "Show.EpisodeList[].ImageUrl",
    type: "PodcastPublic",
  },
  {
    path: "Show.EpisodeList[].PodcastShow.MainImageFileKey",
    output: "Show.EpisodeList[].PodcastShow.ImageUrl",
    type: "PodcastPublic",
  },
];

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

const ACCENT = "#aee339";

const formatVND = (n: number) =>
  n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });

const cycleSuffix = (cycleName: string) => {
  const n = (cycleName || "").toLowerCase();
  if (n.includes("month")) return "/month";
  if (n.includes("year") || n.includes("annual")) return "/year";
  return "/cycle";
};

// Helper function to format duration
const formatDuration = (seconds: number): string => {
  const minutes = Math.floor(seconds / 60);
  return `${minutes} min`;
};

// Helper function to format time ago
const getTimeAgo = (dateString: string): string => {
  console.log("Date String: ", dateString);
  const date = new Date(dateString);
  const now = new Date();
  const diffInDays = Math.floor(
    (now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24)
  );

  if (diffInDays === 0) return "Today";
  if (diffInDays === 1) return "1 day ago";
  return `${diffInDays} days ago`;
};

const ShowDetailsPage = () => {
  // STATES
  const [show, setShow] = useState<ShowDetailsUI | null>(null);
  const [isFileResolving, setIsFileResolving] = useState(false);
  const [isReviewDialogOpen, setIsReviewDialogOpen] = useState(false);
  const [rating, setRating] = useState(0);
  const [hover, setHover] = useState(0);
  const [reviewTitle, setReviewTitle] = useState("");
  const [reviewContent, setReviewContent] = useState("");
  const [isUserSubscribed, setIsUserSubscribed] = useState(false);
  const [isSubscriptionDialogOpen, setIsSubscriptionDialogOpen] =
    useState(false);
  const [isCancelConfirmDialogOpen, setIsCancelConfirmDialogOpen] =
    useState(false);
  const [currentSubscription, setCurrentSubscription] = useState<any | null>(
    null
  );
  const [isFollowed, setIsFollowed] = useState(false);

  // HOOKS
  const user = useSelector((state: RootState) => state.auth.user);

  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  // Mutations
  const [subscribeShow, { isLoading: isSubscribing }] =
    useSubscribePodcastSubscriptionMutation();
  const [unsubscribeShow, { isLoading: isUnsubscribing }] =
    useUnsubscribePodcastSubscriptionMutation();
  const [ratingShow, { isLoading: isRating }] = useRatingShowMutation();

  // Queries
  const {
    data: showDetailsRaw,
    isLoading: isShowDetailsLoading,
    refetch: refetchShowDetails,
  } = useGetShowDetailsQuery({ PodcastShowId: id! }, { skip: !id });

  const {
    data: activeSubscriptionRaw,
    isLoading: isActiveSubscriptionLoading,
    refetch: refetchActiveSubscription,
  } = useGetActiveShowSubscriptionQuery({ ShowId: id! }, { skip: !id });

  const {
    data: customerRegistrationInfo,
    isLoading: isCustomerRegistrationInfoLoading,
    refetch: refetchCustomerRegistration,
  } = useGetCustomerRegistrationInfoFromShowQuery(
    { PodcastShowId: id! },
    { skip: !id }
  );

  const [followShow, { isLoading: isFollowing }] = useFollowShowMutation();
  const [unFollowShow, { isLoading: isUnFollowing }] =
    useUnFollowShowMutation();

  useEffect(() => {
    const resolveData = async () => {
      if (!id) {
        navigate("/media-player/shows");
        return;
      }

      // Wait for API to finish loading
      if (isShowDetailsLoading) {
        return;
      }

      if (!showDetailsRaw) {
        console.log("Show not found");
        return;
      }

      setIsFileResolving(true);
      setIsFollowed(showDetailsRaw.Show.IsFollowedByCurrentUser);
      // Check coi user đã subscribe channel này chưa
      if (
        customerRegistrationInfo &&
        customerRegistrationInfo.PodcastSubscriptionRegistration
      ) {
        if (
          customerRegistrationInfo.PodcastSubscriptionRegistration
            ?.PodcastSubscriptionId ===
          activeSubscriptionRaw?.PodcastSubscription.Id
        ) {
          setIsUserSubscribed(true);
        } else {
          setIsUserSubscribed(false);
        }
      } else {
        setIsUserSubscribed(false);
      }

      // Resolve Show Files
      const { resolvedData: resolvedShow } = await resolveFiles(
        showDetailsRaw,
        ShowFileConfig
      );

      const data = resolvedShow as unknown as { Show: ShowDetailsUI };

      // Process subscription data
      if (activeSubscriptionRaw && activeSubscriptionRaw.PodcastSubscription) {
        setCurrentSubscription(activeSubscriptionRaw.PodcastSubscription);
      } else {
        setCurrentSubscription(null);
      }

      setShow(data.Show);
      console.log("[Show Details]", data.Show);

      setIsFileResolving(false);
    };

    resolveData();
  }, [
    id,
    showDetailsRaw,
    isShowDetailsLoading,
    navigate,
    activeSubscriptionRaw,
    customerRegistrationInfo,
  ]);

  // FUNCTIONS
  // Calculate rating from ReviewList
  const calculateRating = () => {
    if (!show || !show.ReviewList || show.ReviewList.length === 0) {
      return { averageRating: 0, ratingCount: 0 };
    }
    const totalRating = show.ReviewList.reduce(
      (sum, review) => sum + review.Rating,
      0
    );
    const averageRating = totalRating / show.ReviewList.length;
    return { averageRating, ratingCount: show.ReviewList.length };
  };

  const { averageRating, ratingCount } = calculateRating();

  // Check if user already reviewed this show
  const hasUserReviewed = () => {
    if (!user || !show || !show.ReviewList) return false;
    return show.ReviewList.some((review) => review.Account.Id === user.Id);
  };

  const handleUnsubscribeShow = async () => {
    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to perform this action!",
          autoClose: 10,
        })
      );
      return;
    }
    if (
      !customerRegistrationInfo ||
      !customerRegistrationInfo.PodcastSubscriptionRegistration
    ) {
      dispatch(
        setError({
          message:
            "You need to subscribe this show first to perform this action!",
          autoClose: 10,
        })
      );
      return;
    }
    if (!activeSubscriptionRaw) {
      dispatch(
        setError({
          message: "This show doesn't have any subscription to be cancelled!",
          autoClose: 10,
        })
      );
      return;
    }

    try {
      await unsubscribeShow({
        PodcastSubscriptionRegistrationId:
          customerRegistrationInfo.PodcastSubscriptionRegistration.Id,
      }).unwrap();

      // Success thì fetch lại hết
      await Promise.all([
        refetchShowDetails(),
        refetchActiveSubscription(),
        refetchCustomerRegistration(),
      ]);
    } catch (error) {
      dispatch(
        setError({
          message: `Error while cancel subscription: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  const handleSubscribeShow = async (cycleTypeId: number) => {
    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to perform this action!",
          autoClose: 10,
        })
      );
      return;
    }
    if (
      customerRegistrationInfo &&
      customerRegistrationInfo.PodcastSubscriptionRegistration
    ) {
      console.log("Nè: ", customerRegistrationInfo);
      dispatch(
        setError({
          message: "You have already subscribe this Show!",
          autoClose: 10,
        })
      );
      return;
    }
    if (!activeSubscriptionRaw) {
      dispatch(
        setError({
          message: "This show doesn't have any subscription to be subscribed!",
          autoClose: 10,
        })
      );
      return;
    }
    try {
      await subscribeShow({
        CycleTypeId: cycleTypeId,
        PodcastSubscriptionId: activeSubscriptionRaw.PodcastSubscription.Id,
      }).unwrap();

      // Thành công thì fetch lại hết
      await Promise.all([
        refetchShowDetails(),
        refetchActiveSubscription(),
        refetchCustomerRegistration(),
      ]);

      setIsSubscriptionDialogOpen(false);
    } catch (error) {
      dispatch(
        setError({
          message: `Error while subscribing: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  const handleRatingShow = async () => {
    // Validate user login
    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to write a review!",
          autoClose: 10,
        })
      );
      return;
    }

    // Validate rating
    if (rating === 0) {
      dispatch(
        setError({
          message: "Please select a rating from 1 to 5 stars!",
          autoClose: 10,
        })
      );
      return;
    }

    // Validate title
    if (!reviewTitle.trim()) {
      dispatch(
        setError({
          message: "Please enter a review title!",
          autoClose: 10,
        })
      );
      return;
    }

    // Validate content
    if (!reviewContent.trim()) {
      dispatch(
        setError({
          message: "Please write your review content!",
          autoClose: 10,
        })
      );
      return;
    }

    // Validate show ID
    if (!id) {
      dispatch(
        setError({
          message: "Show information is missing!",
          autoClose: 10,
        })
      );
      return;
    }

    try {
      await ratingShow({
        PodcastShowId: id,
        Title: reviewTitle.trim(),
        Content: reviewContent.trim(),
        Rating: rating,
      }).unwrap();

      // Success - refetch show details to update reviews
      await refetchShowDetails();

      // Close dialog and reset form
      setIsReviewDialogOpen(false);
      setRating(0);
      setReviewTitle("");
      setReviewContent("");
      setHover(0);

      // Show success message
      // dispatch(
      //   setError({
      //     message: "Review submitted successfully!",
      //     autoClose: 5,
      //   })
      // );
    } catch (error) {
      dispatch(
        setError({
          message: `Error while submitting review: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  const handlePlayEpisode = (episodeId: string) => {
    dispatch(
      playAudio({
        sourceType: "SpecifyShowEpisodes",
        audioId: episodeId,
      })
    );
  };

  const player = useSelector((state: RootState) => state.player);

  const handleFollow = async (follow: boolean) => {
    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to follow a show!",
          autoClose: 10,
        })
      );
      return;
    }
    try {
      setIsFollowed(follow);
      if (follow) {
        await followShow({ PodcastShowId: id! }).unwrap();
      } else {
        await unFollowShow({ PodcastShowId: id! }).unwrap();
      }
      // Refetch show details after follow/unfollow
      await refetchShowDetails();
    } catch (error) {
      setIsFollowed(!follow);
      dispatch(
        setError({
          message: `Error while ${
            follow ? "following" : "unfollowing"
          } show: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  // RENDER
  if (isShowDetailsLoading || isFileResolving) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="font-poppins font-bold text-[#d9d9d9]">Loading Show...</p>
      </div>
    );
  }

  if (!show) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <LiaDizzy size={100} className="text-[#D9D9D9] animate-bounce" />
        <p className="font-poppins font-bold text-[#d9d9d9]">
          Show Not Found...
        </p>
      </div>
    );
  }

  return (
    <div className="min-h-screen text-white py-6">
      {/* Back Button */}
      <button
        onClick={() => navigate(-1)}
        className="px-12 flex items-center text-white  mb-6 cursor-pointer hover:-translate-y-1 "
      >
        <svg
          className="w-5 h-5 mr-2"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M15 19l-7-7 7-7"
          />
        </svg>
        Previous
      </button>

      {/* Header Section */}
      <div className="flex gap-10 mb-8 px-12 ">
        {/* Show Image */}
        <div className="w-80 h-80 bg-gray-800 rounded-lg overflow-hidden flex-shrink-0">
          <img
            src={show.ImageUrl}
            alt={show.Name}
            className="w-full h-full object-cover"
            onError={(e) => {
              e.currentTarget.style.display = "none";
              e.currentTarget.parentElement!.innerHTML = `
                <div class="w-full h-full bg-gradient-to-br from-gray-700 to-gray-800 flex items-center justify-center">
                  <svg class="w-24 h-24 text-gray-500" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clip-rule="evenodd"/>
                  </svg>
                </div>
              `;
            }}
          />
        </div>

        {/* Show Info */}
        <div className="flex-1 flex flex-col items-start justify-between">
          <h1 className="text-4xl font-medium text-white">{show.Name}</h1>
          <p className="text-xl text-white">{show.Podcaster.FullName}</p>
          <span className="text-sm text-white">
            ⭐ {averageRating.toFixed(1)} ({ratingCount}) -{" "}
            {show.PodcastCategory.Name} - {show.PodcastSubCategory.Name}
          </span>
          {/* <p className="text-gray-300 text-base leading-relaxed my-6 max-w-2xl line-clamp-4">
            {show.Description}
          </p> */}
          <div
            className="text-gray-300 text-base leading-relaxed my-6 max-w-2xl line-clamp-4"
            dangerouslySetInnerHTML={{
              __html: renderDescriptionHTML(show.Description),
            }}
          />

          {/* Action Buttons */}
          <div className="flex w-full items-center justify-between">
            {isUserSubscribed ? (
              <Button className="bg-mystic-green hover:bg-lime-400 transition-all duration-700 ease-out  hover:-translate-y-1 cursor-pointer  text-black font-semibold px-6 py-2 rounded-sm">
                <FaPlay />
                Latest Episode
              </Button>
            ) : (
              <Button className="bg-mystic-green hover:bg-lime-400 transition-all duration-700 ease-out  hover:-translate-y-1 cursor-pointer  text-black font-semibold px-6 py-2 rounded-sm">
                <FaPlay />
                Trailer Audio
              </Button>
            )}
            <div className="flex items-center gap-5">
              {isUserSubscribed ? (
                <LiquidButton
                  variant="minimal"
                  onClick={() => setIsCancelConfirmDialogOpen(true)}
                >
                  <p>
                    {isUnsubscribing ? "Processing..." : "Cancel Subscription"}
                  </p>
                </LiquidButton>
              ) : (
                <LiquidButton
                  variant="minimal"
                  onClick={() => setIsSubscriptionDialogOpen(true)}
                >
                  <p>Subscription Informations</p>
                </LiquidButton>
              )}
            </div>
          </div>

          <div className="absolute w-10 h-10 z-20 top-12 right-12 flex items-center justify-center p-2 rounded-full bg-white/20">
            {isFollowed ? (
              <IoHeartSharp
                size={20}
                className="text-mystic-green cursor-pointer hover:scale-110 transition"
                onClick={() => handleFollow(false)}
              />
            ) : (
              <IoHeartOutline
                size={20}
                className="text-white cursor-pointer hover:scale-110 transition"
                onClick={() => handleFollow(true)}
              />
            )}
          </div>
        </div>
      </div>

      {/* Episodes Section */}
      <div>
        <h2 className="text-2xl font-medium mb-8 mt-12 px-12 ">Episodes</h2>
        <div className="space-y-10 px-3">
          {show.EpisodeList.map((episode) => (
            <div
              key={episode.Id}
              className="px-12 flex h-28 items-center gap-10 p-2 rounded-lg hover:bg-white/10  transition-colors group cursor-pointer"
            >
              <div className="relative aspect-square h-full bg-gray-700 rounded-lg overflow-hidden flex-shrink-0 ">
                <img
                  src={episode.ImageUrl}
                  alt={episode.Name}
                  className="w-full h-full aspect-square object-cover"
                  onError={(e) => {
                    e.currentTarget.style.display = "none";
                    e.currentTarget.parentElement!.innerHTML = `
                      <div class="w-full h-full bg-gradient-to-br from-gray-600 to-gray-700 flex items-center justify-center">
                        <svg class="w-6 h-6 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                          <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clip-rule="evenodd"/>
                        </svg>
                      </div>
                    `;
                  }}
                />

                {player.playMode.playStatus === "play" ? (
                  player.currentAudio?.Id === episode.Id ? (
                    <div className="absolute inset-0 flex bg-black/30 items-center justify-center">
                      <div
                        onClick={() => dispatch(pauseAudio())}
                        className="p-3 rounded-full bg-mystic-green flex items-center justify-center hover:bg-mystic-green "
                      >
                        <PlayingWave />
                      </div>
                    </div>
                  ) : (
                    <div className="absolute inset-0 hidden group-hover:inline-flex bg-black/30 items-center justify-center">
                      <div
                        onClick={() => handlePlayEpisode(episode.Id)}
                        className="p-2  rounded-full bg-gray-400 flex items-center justify-center hover:bg-mystic-green "
                      >
                        <IoPlay size={25} color="#ffffff" />
                      </div>
                    </div>
                  )
                ) : (
                  <div className="absolute inset-0 hidden group-hover:inline-flex bg-black/30 items-center justify-center">
                    <div
                      onClick={() => handlePlayEpisode(episode.Id)}
                      className="p-2  rounded-full bg-gray-400 flex items-center justify-center hover:bg-mystic-green "
                    >
                      <IoPlay size={25} color="#ffffff" />
                    </div>
                  </div>
                )}
              </div>
              <div className="flex-1 flex items-center justify-between gap-20">
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm text-[#d9d9d9] mb-1">
                        {getTimeAgo(episode.ReleaseDate)}
                      </p>
                      <h4 className="font-bold text-lg text-white mb-2 leading-tight">
                        {episode.Name}
                      </h4>
                      {/* <p className="text-white font-light text-sm line-clamp-2 leading-relaxed">
                        {episode.Description}
                      </p> */}
                      <div
                        className="text-white font-light text-sm line-clamp-2 leading-relaxed"
                        dangerouslySetInnerHTML={{
                          __html: renderDescriptionHTML(episode.Description),
                        }}
                      ></div>
                    </div>
                  </div>
                </div>

                <p className="text-white font-bold text-sm">
                  {formatDuration(episode.AudioLength)}
                </p>
                <button className=" text-mystic-green cursor-pointer">
                  <svg
                    className="w-5 h-5"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
                  </svg>
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
      {/* Ratings & Reviews Section */}
      <div className="px-12 w-full">
        <div className="flex items-center gap-4 mb-8 mt-16">
          <h2 className="text-2xl font-medium">Ratings & Reviews</h2>
          {!hasUserReviewed() && (
            <Dialog
              open={isReviewDialogOpen}
              onOpenChange={setIsReviewDialogOpen}
            >
              <DialogTrigger asChild>
                <button className="cursor-pointer w-8 h-8 rounded-full bg-mystic-green hover:bg-lime-400 flex items-center justify-center transition-all duration-300 hover:scale-110">
                  <FaPlus className="text-black" size={16} />
                </button>
              </DialogTrigger>
              <DialogContent className="z-[9999] backdrop-blur-md bg-white/10 border border-white/20 rounded-2xl p-6 shadow-xl">
                <DialogHeader>
                  <DialogTitle className="text-2xl font-semibold text-white mb-2">
                    Write a Review
                  </DialogTitle>
                </DialogHeader>

                {/* Rating & Review Form */}
                <div className="mt-4 w-full ">
                  <div className="mb-4">
                    <label className="block text-white text-sm font-medium mb-3">
                      Your Rating
                    </label>
                    <div className="flex gap-1">
                      {Array.from({ length: 5 }).map((_, i) => {
                        const index = i + 1;
                        return (
                          <button
                            key={index}
                            type="button"
                            onClick={() => setRating(index)}
                            onMouseEnter={() => setHover(index)}
                            onMouseLeave={() => setHover(0)}
                            className="w-10 h-10 transition-transform duration-200 hover:scale-110"
                          >
                            <svg
                              className={`w-full h-full cursor-pointer transition-colors duration-200 ${
                                index <= (hover || rating)
                                  ? "text-yellow-400"
                                  : "text-gray-400"
                              }`}
                              fill="currentColor"
                              viewBox="0 0 20 20"
                            >
                              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                          </button>
                        );
                      })}
                    </div>
                  </div>

                  {/* Title Input */}
                  <div className="mb-4">
                    <label className="block text-white text-sm font-medium mb-2">
                      Review Title
                    </label>
                    <input
                      type="text"
                      value={reviewTitle}
                      onChange={(e) => setReviewTitle(e.target.value)}
                      placeholder="Give your review a title..."
                      className="w-full px-4 py-3 bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-mystic-green/50 focus:border-mystic-green/50 transition-all duration-300"
                    />
                  </div>

                  {/* Content Textarea */}
                  <div className="mb-6">
                    <label className="block text-white text-sm font-medium mb-2">
                      Review Content
                    </label>
                    <textarea
                      rows={5}
                      value={reviewContent}
                      onChange={(e) => setReviewContent(e.target.value)}
                      placeholder="Share your thoughts about this podcast..."
                      className="w-full px-4 py-3 bg-white/5 backdrop-blur-sm border border-white/20 rounded-xl text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-mystic-green/50 focus:border-mystic-green/50 transition-all duration-300 resize-none"
                    />
                  </div>

                  {/* Submit Button */}
                  <Button
                    onClick={handleRatingShow}
                    disabled={isRating}
                    className="w-full bg-mystic-green hover:bg-lime-400 text-black font-semibold py-3 rounded-xl transition-all duration-300 hover:shadow-lg hover:shadow-mystic-green/25 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {isRating ? "Submitting..." : "Submit Review"}
                  </Button>
                </div>
              </DialogContent>
            </Dialog>
          )}
        </div>

        <div className="flex gap-12 mb-14 w-full items-center">
          {/* Left side - Overall Rating */}
          <div className="gap-12 w-full flex items-center">
            <div className="">
              <p className="text-6xl font-bold text-mystic-green">
                {averageRating.toFixed(1)}
              </p>
              <p className="text-white text-center text-lg  mb-1">Out of 5</p>
            </div>

            {/* Rating bars */}
            <div className="space-y-2 w-100">
              {[5, 4, 3, 2, 1].map((rating) => {
                const count = show.ReviewList.filter(
                  (r) => Math.floor(r.Rating) === rating
                ).length;
                const percentage =
                  show.ReviewList.length > 0
                    ? (count / show.ReviewList.length) * 100
                    : 0;

                return (
                  <div key={rating} className="flex items-center gap-3 h-[1em]">
                    <div className="flex min-w-20">
                      {Array.from({ length: rating }).map((_, i) => (
                        <svg
                          key={i}
                          className="w-4 h-4 text-mystic-green"
                          fill="currentColor"
                          viewBox="0 0 20 20"
                        >
                          <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                        </svg>
                      ))}
                    </div>

                    <div className="flex-1 bg-gray-300/20 h-2 rounded-full overflow-hidden">
                      <div
                        className="bg-white h-full transition-all duration-300"
                        style={{ width: `${percentage}%` }}
                      ></div>
                    </div>

                    <span className="text-white text-sm min-w-[3rem] text-right">
                      {count}
                    </span>
                  </div>
                );
              })}
              <div className="text-white text-xs font-light text-right mt-4">
                {ratingCount.toLocaleString()} ratings
              </div>
            </div>
          </div>
        </div>
        <Carousel
          opts={{
            align: "start",
            loop: false,
          }}
          className="w-full"
        >
          <CarouselContent className="-ml-4">
            {show.ReviewList.map((review) => (
              <CarouselItem key={review.Id} className="pl-4 basis-1/3">
                <div
                  className=" rounded-2xl p-6 h-full "
                  style={{ backgroundColor: "rgba(255, 255, 255, 0.1)" }}
                >
                  <div className="flex justify-between items-start mb-2">
                    <div className="text-xs text-gray-200">
                      {getTimeAgo(review.UpdatedAt)}
                    </div>
                    <div className="text-xs text-gray-200">
                      {review.Account.FullName}
                    </div>
                  </div>

                  <h4 className="font-semibold text-white text-base mb-3">
                    {review.Title}
                  </h4>

                  <div className="flex mb-4">
                    {Array.from({ length: 5 }).map((_, i) => (
                      <svg
                        key={i}
                        className={`w-4 h-4 ${
                          i < Math.floor(review.Rating)
                            ? "text-yellow-400"
                            : "text-gray-400"
                        }`}
                        fill="currentColor"
                        viewBox="0 0 20 20"
                      >
                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                      </svg>
                    ))}
                  </div>

                  <p className="text-gray-100 text-sm leading-relaxed">
                    {review.Content}
                  </p>
                </div>
              </CarouselItem>
            ))}
          </CarouselContent>
        </Carousel>
      </div>

      {/* Information Section */}
      <div className="px-12">
        <h2 className="text-2xl font-medium mb-8 mt-14">Information</h2>

        {/* Grid Layout - 3 columns */}
        <div className="grid grid-cols-3 gap-x-16 gap-y-6 mb-8">
          {/* Creator */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Creator</h3>
            <p className="text-white text-base">{show.Podcaster.FullName}</p>
          </div>

          {/* Seasons */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Seasons</h3>
            <p className="text-white text-base">
              {show.EpisodeList[0]?.SeasonNumber || 1}
            </p>
          </div>

          {/* Rating */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Upload Frequency</h3>
            <p className="text-white text-base">{show.UploadFrequency}</p>
          </div>

          {/* Copyright */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Copyright</h3>
            <p className="text-white text-base">{show.Copyright}</p>
          </div>

          {/* Show Website */}

          {/* Provider */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Release Date</h3>
            <p className="text-white text-base">{show.ReleaseDate}</p>
          </div>
        </div>

        {/* Full Description */}
        <div className="mt-8 pt-8 border-t border-white/10">
          <p className="text-white text-base leading-relaxed">
            {show.Description}
          </p>
        </div>
      </div>

      {/* Cancel Subscription Confirmation Dialog */}
      {isUserSubscribed && currentSubscription && (
        <Dialog
          open={isCancelConfirmDialogOpen}
          onOpenChange={setIsCancelConfirmDialogOpen}
        >
          <DialogContent
            className="w-[450px] px-8 py-8 border border-white/10 bg-[#0f1115]/50 text-white
                 backdrop-blur-xl shadow-2xl rounded-2xl"
          >
            <DialogHeader>
              <DialogTitle className="text-2xl font-bold text-white">
                Cancel Subscription?
              </DialogTitle>
              <DialogDescription className="text-white/70 mt-4 space-y-3">
                {currentSubscription.PodcastChannelId &&
                !currentSubscription.PodcastShowId ? (
                  <>
                    <p className="font-semibold text-mystic-green">
                      ⚠️ Important Notice
                    </p>
                    <p>
                      This subscription belongs to the entire{" "}
                      <span className="font-bold text-mystic-green">
                        Channel
                      </span>
                      . Canceling this subscription will cancel your access to
                      all shows in this channel, not just this show.
                    </p>
                  </>
                ) : (
                  <p>
                    Are you sure you want to cancel your subscription to this
                    show? You will lose access to all premium content.
                  </p>
                )}
              </DialogDescription>
            </DialogHeader>
            <DialogFooter className="mt-6 flex gap-3 flex-row justify-end">
              <Button
                variant="outline"
                onClick={() => setIsCancelConfirmDialogOpen(false)}
                className="px-6 py-2 border-white bg-white text-black hover:bg-white/10"
              >
                Keep Subscription
              </Button>
              <Button
                onClick={() => {
                  setIsCancelConfirmDialogOpen(false);
                  handleUnsubscribeShow();
                }}
                className="px-6 py-2 bg-red-600 hover:bg-red-700 text-white"
              >
                Yes, Cancel
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}

      {/* Subscription Dialog */}
      {!isUserSubscribed && currentSubscription && (
        <Dialog
          open={isSubscriptionDialogOpen}
          onOpenChange={setIsSubscriptionDialogOpen}
        >
          <DialogContent
            className="w-[500px] px-8 py-12 border border-white/10 bg-[#0f1115]/50 text-white
                 backdrop-blur-xl shadow-2xl rounded-2xl"
          >
            <DialogHeader>
              <DialogTitle className="text-3xl text-mystic-green font-bold tracking-tight">
                {currentSubscription?.Name}
              </DialogTitle>
              <DialogDescription className="text-white/70 mt-2">
                <div className="flex flex-col items-start">
                  {currentSubscription.PodcastChannelId &&
                    !currentSubscription.PodcastShowId && (
                      <p>
                        This Subscription is belongs to the{" "}
                        <span className="font-bold text-mystic-green">
                          Channel
                        </span>
                      </p>
                    )}
                  {!currentSubscription.PodcastChannelId &&
                    currentSubscription.PodcastShowId && (
                      <p>
                        This Subscription is belongs to this own{" "}
                        <span className="font-bold text-mystic-green">
                          Show
                        </span>
                      </p>
                    )}
                  <p>{currentSubscription?.Description}</p>
                </div>
              </DialogDescription>
            </DialogHeader>

            <Tabs
              defaultValue={
                currentSubscription.PodcastSubscriptionCycleTypePriceList[0]
                  .SubscriptionCycleType.Name
              }
              className="w-full mt-4"
            >
              <TabsList
                className="w-full transition-all duration-200 ease-out flex items-center md:inline-flex gap-2 bg-white/5 p-1 rounded-full
                     ring-1 ring-white/10"
              >
                {currentSubscription.PodcastSubscriptionCycleTypePriceList.map(
                  (d: any) => (
                    <TabsTrigger
                      key={d.SubscriptionCycleType.Id}
                      value={d.SubscriptionCycleType.Name}
                      className="data-[state=active]:bg-[var(--accent)]
                         data-[state=active]:text-black data-[state=active]:shadow
                         rounded-full px-5 py-2 text-sm font-semibold
                         text-white
                         hover:bg-white/10 transition"
                      style={{ ["--accent" as any]: ACCENT }}
                    >
                      {d.SubscriptionCycleType.Name}
                    </TabsTrigger>
                  )
                )}
              </TabsList>

              {currentSubscription.PodcastSubscriptionCycleTypePriceList.map(
                (d: any) => (
                  <TabsContent
                    key={d.SubscriptionCycleType.Id}
                    value={d.SubscriptionCycleType.Name}
                    className="mt-6 space-y-6"
                  >
                    <div
                      className="rounded-2xl p-6 md:p-8 border border-white/10
                         bg-gradient-to-b from-white/5 to-transparent"
                    >
                      <div className="flex items-end gap-3">
                        <span className="text-4xl md:text-5xl text-mystic-green font-extrabold leading-none">
                          {formatVND(d.Price)}đ
                        </span>
                        <span className="text-white/60 mb-1">
                          {cycleSuffix(d.SubscriptionCycleType.Name)}
                        </span>
                      </div>

                      {currentSubscription.PodcastSubscriptionBenefitMappingList
                        .length > 0 && (
                        <ul className="mt-5 flex flex-col gap-3">
                          {currentSubscription.PodcastSubscriptionBenefitMappingList.map(
                            (b: any) => (
                              <li
                                key={
                                  b.PodcastSubscriptionId -
                                  b.PodcastSubscriptionBenefit.Id
                                }
                                className="flex items-start gap-3"
                              >
                                <span
                                  className="mt-0.5 inline-flex h-5 w-5 items-center justify-center rounded-full
                                   ring-1 ring-white/15"
                                  style={{
                                    backgroundColor: "rgba(174,227,57,0.15)",
                                    color: "#cde97a",
                                  }}
                                >
                                  <Check size={14} />
                                </span>
                                <span className="text-sm text-white/90">
                                  {b.PodcastSubscriptionBenefit.Name}
                                </span>
                              </li>
                            )
                          )}
                        </ul>
                      )}

                      <DialogFooter className="mt-8 flex items-center justify-center">
                        <Button
                          onClick={() =>
                            handleSubscribeShow(d.SubscriptionCycleType.Id)
                          }
                          className="w-full mx-auto md:w-auto font-bold rounded-xl px-6 py-6
                             text-black hover:brightness-95"
                          style={{ backgroundColor: ACCENT }}
                        >
                          Subscribe now for only {formatVND(d.Price)}đ
                          {cycleSuffix(d.SubscriptionCycleType.Name)}
                        </Button>
                      </DialogFooter>
                    </div>
                  </TabsContent>
                )
              )}
            </Tabs>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
};

export default ShowDetailsPage;
