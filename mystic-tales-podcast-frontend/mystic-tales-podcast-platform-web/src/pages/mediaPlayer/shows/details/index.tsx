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
  DialogClose,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Check, MoreHorizontalIcon } from "lucide-react";
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
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import type { RootState } from "@/redux/store";
import { useEffect, useState } from "react";
import { FaPlus } from "react-icons/fa6";
import { FaPlay } from "react-icons/fa6";
import { IoHeartOutline, IoHeartSharp } from "react-icons/io5";
import { LiaDizzy } from "react-icons/lia";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate, useParams } from "react-router-dom";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@radix-ui/react-dropdown-menu";
import { TbMessageReport } from "react-icons/tb";
import {
  useGetShowReportTypesQuery,
  useReportShowMutation,
} from "@/core/services/report/report.service";
import EpisodeCard from "./components/EpisodeCard";
import { MdKeyboardArrowRight } from "react-icons/md";
import { setSeeMoreEpisodeData } from "@/redux/slices/seeMoreEpisodeSlice/seeMoreEpisodeSlice";
import { useLazyGetPodcastPublicSourceQuery } from "@/core/services/file/file.service";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

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

  // TRAILER AUDIO
  const [isPlayingTrailer, setIsPlayingTrailer] = useState(false);
  const [trailerAudio, setTrailerAudio] = useState<HTMLAudioElement | null>(
    null
  );

  // REPORT
  const [reportShowDialog, setReportShowDialog] = useState(false);
  const [isShowAlreadyReported, setIsShowAlreadyReported] = useState(false);
  const [showSelectedReportTypeId, setShowSelectedReportTypeId] = useState<
    number | null
  >(null);
  const [showReportContent, setShowReportContent] = useState("");

  // HOOKS
  const user = useSelector((state: RootState) => state.auth.user);

  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  // Mutations
  const [subscribeShow] = useSubscribePodcastSubscriptionMutation();
  const [unsubscribeShow, { isLoading: isUnsubscribing }] =
    useUnsubscribePodcastSubscriptionMutation();
  const [ratingShow, { isLoading: isRating }] = useRatingShowMutation();

  const [reportShow, { isLoading: isReportingShow }] = useReportShowMutation();

  // Queries
  const {
    data: show,
    isLoading: isShowDetailsLoading,
    refetch: refetchShowDetails,
  } = useGetShowDetailsQuery({ PodcastShowId: id! }, { skip: !id });

  const { data: activeSubscriptionRaw, refetch: refetchActiveSubscription } =
    useGetActiveShowSubscriptionQuery({ ShowId: id! }, { skip: !id });

  const {
    data: customerRegistrationInfo,
    refetch: refetchCustomerRegistration,
  } = useGetCustomerRegistrationInfoFromShowQuery(
    { PodcastShowId: id! },
    { skip: !id }
  );

  const {
    data: showAvailableReportTypes,
    isLoading: isShowAvailableReportTypesLoading,
    refetch: refetchShowReportTypes,
  } = useGetShowReportTypesQuery({ PodcastShowId: id! }, { skip: !id });

  const [followShow] = useFollowShowMutation();
  const [unFollowShow] = useUnFollowShowMutation();

  const [getTrailerAudioUrl] = useLazyGetPodcastPublicSourceQuery();
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

      if (!show || !show.Show) {
        console.log("Show not found");
        return;
      }

      setIsFollowed(show.Show.IsFollowedByCurrentUser);
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

      // Check nếu user đã report show này chưa
      if (showAvailableReportTypes) {
        if (showAvailableReportTypes.ShowReportTypeList.length > 0) {
          setIsShowAlreadyReported(false);
        } else {
          setIsShowAlreadyReported(true);
        }
      } else {
        setIsShowAlreadyReported(true);
      }

      // Sort episodes: newest first by SeasonNumber, EpisodeOrder, then ReleaseDate
      if (show && show.Show && Array.isArray(show.Show.EpisodeList)) {
        const sortedEpisodes = [...show.Show.EpisodeList].sort((a, b) => {
          const seasonDiff = (b.SeasonNumber ?? 0) - (a.SeasonNumber ?? 0);
          if (seasonDiff !== 0) return seasonDiff;

          const orderDiff = (b.EpisodeOrder ?? 0) - (a.EpisodeOrder ?? 0);
          if (orderDiff !== 0) return orderDiff;

          const aTime = a.CreatedAt ? new Date(a.CreatedAt).getTime() : 0;
          const bTime = b.CreatedAt ? new Date(b.CreatedAt).getTime() : 0;
          return bTime - aTime;
        });
        show.Show.EpisodeList = sortedEpisodes;
      }

      // Process subscription data
      if (activeSubscriptionRaw && activeSubscriptionRaw.PodcastSubscription) {
        setCurrentSubscription(activeSubscriptionRaw.PodcastSubscription);
      } else {
        setCurrentSubscription(null);
      }
    };

    resolveData();
  }, [
    id,
    show,
    isShowDetailsLoading,
    navigate,
    activeSubscriptionRaw,
    customerRegistrationInfo,
    showAvailableReportTypes,
    isShowAvailableReportTypesLoading,
  ]);

  // FUNCTIONS
  // Calculate rating from ReviewList
  const calculateRating = () => {
    if (!show || !show.Show.ReviewList || show.Show.ReviewList.length === 0) {
      return { averageRating: 0, ratingCount: 0 };
    }
    const totalRating = show.Show.ReviewList.reduce(
      (sum, review) => sum + review.Rating,
      0
    );
    const averageRating = totalRating / show.Show.ReviewList.length;
    return {
      averageRating,
      ratingCount: show.Show.ReviewList ? show.Show.ReviewList.length : 0,
    };
  };

  const { averageRating, ratingCount } = calculateRating();

  // Check if user already reviewed this show
  const hasUserReviewed = () => {
    if (!user || !show || !show.Show.ReviewList) return false;
    return show.Show.ReviewList.some((review) => review.Account.Id === user.Id);
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

  const handleReportShow = async () => {
    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to report a show!",
          autoClose: 10,
        })
      );
      return;
    }
    if (!showSelectedReportTypeId || showReportContent.trim() === "" || !id) {
      return;
    }
    try {
      await reportShow({
        PodcastShowId: id,
        ReportTypeId: showSelectedReportTypeId,
        Content: showReportContent.trim(),
      }).unwrap();
      // Success - refetch show details to update reports
      await refetchShowDetails();
      await refetchShowReportTypes();

      // Close dialog and reset form
      setReportShowDialog(false);
      setShowSelectedReportTypeId(null);
      setShowReportContent("");
    } catch (error) {
      dispatch(
        setError({
          message: `Error while reporting show: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  const handleSeeMoreEpisodeFromShow = () => {
    if (!show) return;
    dispatch(
      setSeeMoreEpisodeData({
        title: `Episodes from ${show?.Show.Name}`,
        episodes: show.Show.EpisodeList,
      })
    );
    navigate(`/media-player/episodes`);
  };

  const handlePlayTrailerAudio = async () => {
    if (!show || !show.Show.TrailerAudioFileKey) return;

    try {
      // If already playing, stop it
      if (isPlayingTrailer && trailerAudio) {
        trailerAudio.pause();
        trailerAudio.currentTime = 0;
        setIsPlayingTrailer(false);
        setTrailerAudio(null);
        return;
      }

      // Get audio URL and play
      const { data } = await getTrailerAudioUrl({
        FileKey: show.Show.TrailerAudioFileKey,
      });

      if (data && data.FileUrl) {
        const audio = new Audio(data.FileUrl);

        // Set up event listeners
        audio.addEventListener("ended", () => {
          setIsPlayingTrailer(false);
          setTrailerAudio(null);
        });

        audio.addEventListener("error", () => {
          setIsPlayingTrailer(false);
          setTrailerAudio(null);
          dispatch(
            setError({
              message: "Failed to play trailer audio",
              autoClose: 10,
            })
          );
        });

        setTrailerAudio(audio);
        setIsPlayingTrailer(true);
        await audio.play();
      }
    } catch (error) {
      setIsPlayingTrailer(false);
      setTrailerAudio(null);
      dispatch(
        setError({
          message: `Error while playing trailer audio: ${error}`,
          autoClose: 20,
        })
      );
    }
  };

  // Cleanup audio on unmount
  useEffect(() => {
    return () => {
      if (trailerAudio) {
        trailerAudio.pause();
        trailerAudio.currentTime = 0;
      }
    };
  }, [trailerAudio]);

  // RENDER
  if (isShowDetailsLoading) {
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
        <div className="w-80 h-80 bg-gray-800 rounded-lg overflow-hidden shrink-0">
          <AutoResolveImage
            FileKey={show.Show.MainImageFileKey}
            type="PodcastPublicSource"
            className="w-full h-full object-cover"
          />
        </div>

        {/* Show Info */}
        <div className="flex-1 flex flex-col items-start justify-between">
          <h1 className="text-4xl font-medium text-white">{show.Show.Name}</h1>
          <p className="text-xl text-white">{show.Show.Podcaster.FullName}</p>
          <span className="text-sm text-white">
            ⭐ {averageRating.toFixed(1)} (
            {ratingCount !== undefined ? ratingCount : 0})
            {show.Show.PodcastCategory
              ? ` - ${show.Show.PodcastCategory.Name}`
              : "Unknown Category"}{" "}
            {show.Show.PodcastSubCategory
              ? ` - ${show.Show.PodcastSubCategory.Name}`
              : ""}
          </span>
          {/* <p className="text-gray-300 text-base leading-relaxed my-6 max-w-2xl line-clamp-4">
            {show.Description}
          </p> */}
          <div
            className="text-gray-300 text-base leading-relaxed my-6 max-w-2xl line-clamp-4"
            dangerouslySetInnerHTML={{
              __html: renderDescriptionHTML(show.Show.Description),
            }}
          />

          {/* Action Buttons */}
          <div className="flex w-full items-center justify-between">
            <Button
              onClick={() => handlePlayTrailerAudio()}
              disabled={!show.Show.TrailerAudioFileKey}
              className={`${
                isPlayingTrailer
                  ? "bg-white hover:bg-gray-100"
                  : "bg-mystic-green hover:bg-lime-400"
              } transition-all duration-300 ease-out hover:-translate-y-1 cursor-pointer text-black font-semibold px-6 py-2 rounded-sm disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:translate-y-0`}
            >
              {isPlayingTrailer ? (
                <>
                  <svg
                    className="w-4 h-4 mr-2"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path
                      fillRule="evenodd"
                      d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zM7 8a1 1 0 012 0v4a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v4a1 1 0 102 0V8a1 1 0 00-1-1z"
                      clipRule="evenodd"
                    />
                  </svg>
                  Stop Trailer
                </>
              ) : (
                <>
                  <FaPlay className="mr-2" />
                  {show.Show.TrailerAudioFileKey
                    ? "Play Trailer"
                    : "No Trailer Available"}
                </>
              )}
            </Button>
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

          <div className="absolute z-20 top-12 right-12 flex items-center justify-center gap-5">
            {isFollowed ? (
              <div className="w-10 h-10 p-2 rounded-full bg-white/20 flex items-center justify-center">
                <IoHeartSharp
                  size={20}
                  className="text-mystic-green cursor-pointer hover:scale-110 transition"
                  onClick={() => handleFollow(false)}
                />
              </div>
            ) : (
              <div className="w-10 h-10 p-2 rounded-full bg-white/20 flex items-center justify-center">
                <IoHeartOutline
                  size={20}
                  className="text-white cursor-pointer hover:scale-110 transition"
                  onClick={() => handleFollow(true)}
                />
              </div>
            )}

            <DropdownMenu modal={false}>
              <DropdownMenuTrigger asChild>
                <div className="w-10 h-10 p-2 rounded-full bg-white/20 flex items-center justify-center">
                  <MoreHorizontalIcon
                    size={20}
                    className="text-white cursor-pointer hover:scale-110 transition"
                  />
                </div>
              </DropdownMenuTrigger>
              <DropdownMenuContent
                className="w-60 z-9999 py-4 rounded-2xl bg-white/20 backdrop-blur-xl border border-white/30 shadow-md text-white"
                align="end"
                sideOffset={8}
              >
                <p className="ml-4 font-bold text-white mb-3">Show Actions</p>
                <DropdownMenuGroup>
                  <DropdownMenuItem
                    onSelect={() => setReportShowDialog(true)}
                    disabled={false}
                    className=" 
                    mx-2 px-2 py-1 flex items-center justify-start gap-2 rounded-lg cursor-pointer
                    hover:font-semibold hover:shadow-md hover:bg-black/10 transition-all
                    outline-none focus:outline-none focus-visible:outline-none
                    focus:ring-0 focus-visible:ring-0"
                  >
                    <TbMessageReport />
                    <p className="text-sm">Report Show</p>
                  </DropdownMenuItem>
                </DropdownMenuGroup>
              </DropdownMenuContent>
            </DropdownMenu>

            <Dialog open={reportShowDialog} onOpenChange={setReportShowDialog}>
              <DialogContent className="z-9999 sm:max-w-125 bg-[#0f1115]/95 border-white/10 text-white">
                <DialogHeader>
                  <DialogTitle className="text-2xl font-bold text-mystic-green">
                    Report Show
                  </DialogTitle>
                  <DialogDescription className="text-white/70">
                    We truly appreciate your feedback. <br />
                    Please select a report type and share the reason so we can
                    review and improve this show.
                  </DialogDescription>
                </DialogHeader>

                <div className="space-y-4 py-4 gap-10">
                  {/* Report Type Selection */}
                  <div className="space-y-2 mb-5 gap-2">
                    <Label htmlFor="report-type" className="text-white">
                      Report Type <span className="text-red-500">*</span>
                    </Label>
                    {isShowAvailableReportTypesLoading ? (
                      <div className="h-10 bg-white/5 rounded-md animate-pulse" />
                    ) : isShowAlreadyReported ? (
                      <p className="text-sm text-yellow-500">
                        You have already reported this show
                      </p>
                    ) : (
                      <Select
                        value={showSelectedReportTypeId?.toString() || ""}
                        onValueChange={(value) =>
                          setShowSelectedReportTypeId(Number(value))
                        }
                      >
                        <SelectTrigger className="bg-white/5 border-white/10 text-white">
                          <SelectValue placeholder="Select a report type" />
                        </SelectTrigger>
                        <SelectContent className="z-9999 bg-[#1a1d24] border-white/10 text-white">
                          {showAvailableReportTypes?.ShowReportTypeList.map(
                            (type) => (
                              <SelectItem
                                key={type.Id}
                                value={type.Id.toString()}
                                className="focus:bg-white/10 focus:text-white"
                              >
                                {type ? type.Name : "Unknown"}
                              </SelectItem>
                            )
                          )}
                        </SelectContent>
                      </Select>
                    )}
                  </div>

                  {/* Report Content */}
                  <div className="space-y-2 gap-2">
                    <Label htmlFor="report-content" className="text-white">
                      Details <span className="text-red-500">*</span>
                    </Label>
                    <Textarea
                      id="report-content"
                      placeholder="Please provide more details about the issue..."
                      value={showReportContent}
                      onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                        setShowReportContent(e.target.value)
                      }
                      className="min-h-30 bg-white/5 border-white/10 text-white placeholder:text-white/40 resize-none"
                      disabled={isShowAlreadyReported}
                    />
                    <p className="text-xs text-white/50">
                      Minimum 10 characters
                    </p>
                  </div>
                </div>

                <DialogFooter className="gap-2">
                  <DialogClose asChild>
                    <Button
                      variant="outline"
                      className="bg-transparent border-white/20 text-white hover:bg-white/10"
                    >
                      Cancel
                    </Button>
                  </DialogClose>
                  <Button
                    onClick={async () => {
                      await handleReportShow();
                      setReportShowDialog(false);
                    }}
                    disabled={
                      !showSelectedReportTypeId ||
                      showReportContent.trim().length < 10 ||
                      isReportingShow ||
                      isShowAlreadyReported
                    }
                    className="bg-mystic-green text-black hover:bg-mystic-green/90"
                  >
                    {isReportingShow ? "Submitting..." : "Submit Report"}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        </div>
      </div>

      {/* Episodes Section */}
      <div>
        <div className="w-full flex items-center justify-between px-12 mb-8 mt-12">
          <h2 className="text-2xl font-medium">
            Episodes ({show.Show.EpisodeList.length})
          </h2>
          <div
            onClick={() => handleSeeMoreEpisodeFromShow()}
            className="flex items-center gap-2 hover:underline cursor-pointer"
          >
            <p className="font-poppins">See more</p>
            <MdKeyboardArrowRight />
          </div>
        </div>
        <div className="space-y-10 px-3">
          {/* {show.EpisodeList.map((episode) => (
            <EpisodeCard key={episode.Id} episode={episode} />
          ))} */}
          {show.Show.EpisodeList.slice(0, 5).map((episode) => (
            <EpisodeCard key={episode.Id} episode={episode} />
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
              <DialogContent className="z-9999 backdrop-blur-md bg-white/10 border border-white/20 rounded-2xl p-6 shadow-xl">
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
                const count = show.Show.ReviewList.filter(
                  (r) => Math.floor(r.Rating) === rating
                ).length;
                const percentage =
                  show.Show.ReviewList.length > 0
                    ? (count / show.Show.ReviewList.length) * 100
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

                    <span className="text-white text-sm min-w-12 text-right">
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
            {show.Show.ReviewList.map((review) => (
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
            <p className="text-white text-base">{show.Show.Podcaster.FullName}</p>
          </div>

          {/* Seasons */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Seasons</h3>
            <p className="text-white text-base">
              {show.Show.EpisodeList[0]?.SeasonNumber || 1}
            </p>
          </div>

          {/* Rating */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Upload Frequency</h3>
            <p className="text-white text-base">{show.Show.UploadFrequency}</p>
          </div>

          {/* Copyright */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Copyright</h3>
            <p className="text-white text-base">{show.Show.Copyright}</p>
          </div>

          {/* Show Website */}

          {/* Provider */}
          <div>
            <h3 className="text-gray-400 text-sm mb-2">Release Date</h3>
            <p className="text-white text-base">
              {show.Show.IsReleased
                ? show.Show.ReleaseDate
                : `Not Released Yet - Will be release on ${show.Show.ReleaseDate}`}
            </p>
          </div>
        </div>

        {/* Full Description */}
        <div className="mt-8 pt-8 border-t border-white/10">
          <div
            className="text-white text-base leading-relaxed"
            dangerouslySetInnerHTML={{
              __html: renderDescriptionHTML(show.Show.Description),
            }}
          />
        </div>
      </div>

      {/* Cancel Subscription Confirmation Dialog */}
      {isUserSubscribed && currentSubscription && (
        <Dialog
          open={isCancelConfirmDialogOpen}
          onOpenChange={setIsCancelConfirmDialogOpen}
        >
          <DialogContent
            className="w-112.5 px-8 py-8 border border-white/10 bg-[#0f1115]/50 text-white
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
            className="w-125 px-8 py-12 border border-white/10 bg-[#0f1115]/50 text-white
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
                      className="data-[state=active]:bg-accent
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
                         bg-linear-to-b from-white/5 to-transparent"
                    >
                      <div className="flex items-end gap-3">
                        <span className="text-4xl md:text-5xl text-mystic-green font-extrabold leading-none">
                          {formatVND(d.Price)} coins
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
                          Subscribe now for only {formatVND(d.Price)} coins
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
