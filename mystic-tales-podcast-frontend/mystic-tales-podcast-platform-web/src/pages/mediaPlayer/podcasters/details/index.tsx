import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";

import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import { IoIosArrowBack } from "react-icons/io";
import { useNavigate, useParams } from "react-router-dom";
import ChannelCard from "./components/ChannelCard";
import type { ShowFromAPI} from "@/core/types/show";
import type {
  PodcasterReviewAPI,
} from "@/core/types/podcaster";
import type { PodcastCategory } from "@/core/types/podcastCategory";
import ShowsByCategoryCarousel from "./components/ShowsByCategoryCarousel";
import {
  useFollowPodcasterMutation,
  useGetPodcasterDetailsQuery,
  useUnFollowPodcasterMutation,
} from "@/core/services/podcasters/podcasters.service";
import { useGetShowListFromPodcasterQuery } from "@/core/services/show/show.service";
import { useGetChannelListFromPodcasterQuery } from "@/core/services/channel/channel.service";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";
import { LiaDizzy } from "react-icons/lia";
import Loading from "@/components/loading";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import { BiSolidBadgeCheck } from "react-icons/bi";
import RatingChart from "./components/RatingChart";
import ReviewCard from "./components/ReviewCard";
import type { PodcastBuddyFromAPI } from "@/core/types/booking";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

const PodcasterDetailsPage = () => {
  // REDUX
  const user = useSelector((state: RootState) => state.auth.user);

  // PARAMS
  const { id: podcasterId } = useParams<{ id: string }>();

  // STATES
  const [showsByCategory, setShowsByCategory] = useState<
    { Category: PodcastCategory; ShowList: ShowFromAPI[] }[]
  >([]);
  const [isNotFound, setIsNotFound] = useState(false);

  // HOOKS
  const navigate = useNavigate();
  const dispatch = useDispatch();
  // ===============================================
  const [follow, { isLoading: isFollowing }] = useFollowPodcasterMutation();
  const [unfollow] =
    useUnFollowPodcasterMutation();
  // Lấy Podcaster Details
  const { data: podcasterDetailsRaw, isLoading: isPodcasterDetailsLoading } =
    useGetPodcasterDetailsQuery(
      { podcasterId: Number(podcasterId) },
      { skip: !podcasterId }
    );
  // Lấy danh sách id các Podcaster đã followed của Customer

  // Lấy danh sách các channel và show từ podcaster details
  const { data: showsRaw, isLoading: isShowsLoading } =
    useGetShowListFromPodcasterQuery(
      { podcasterId: Number(podcasterId) },
      { skip: !podcasterId }
    );
  const { data: channelsRaw, isLoading: isChannelsLoading } =
    useGetChannelListFromPodcasterQuery(
      { podcasterId: Number(podcasterId) },
      { skip: !podcasterId }
    );

  // Handle Resolve File Ở Đây
  useEffect(() => {
    const resolveData = async () => {
      if (!podcasterId) {
        navigate("/media-player/podcasters");
        return;
      }

      // Đợi API loading xong
      if (isPodcasterDetailsLoading || isShowsLoading || isChannelsLoading) {
        return;
      }

      // ===== USE API DATA =====
      if (!podcasterDetailsRaw) {
        console.log("Không tìm thấy podcaster");
        setIsNotFound(true);
        return;
      }


      // Resolve Shows
      if (showsRaw && showsRaw.ShowList) {
        // Group shows by category
        const showsGroupedByCategory = (
          showsRaw.ShowList as ShowFromAPI[]
        ).reduce((acc, show) => {
          const categoryId = show.PodcastCategory.Id;
          const existingCategory = acc.find(
            (item) => item.Category.Id === categoryId
          );

          if (existingCategory) {
            existingCategory.ShowList.push(show);
          } else {
            acc.push({
              Category: show.PodcastCategory,
              ShowList: [show],
            });
          }

          return acc;
        }, [] as { Category: PodcastCategory; ShowList: ShowFromAPI[] }[]);

        setShowsByCategory(showsGroupedByCategory);
      } else {
        setShowsByCategory([]);
      }
    };

    resolveData();
  }, [
    podcasterDetailsRaw,
    showsRaw,
    channelsRaw,
    podcasterId,
    isPodcasterDetailsLoading,
    isShowsLoading,
    isChannelsLoading,
    navigate,
  ]);

  const handleCreateBooking = () => {
    if (!podcasterDetailsRaw) {
      return;
    } else {
      const payload: PodcastBuddyFromAPI = {
        Id: podcasterDetailsRaw.AccountId,
        FullName: podcasterDetailsRaw.Name,
        MainImageFileKey: podcasterDetailsRaw.MainImageFileKey,
        AverageRating: podcasterDetailsRaw.AverageRating,
        TotalBookingCompleted: 0,
        TotalFollow: 0,
        PriceBookingPerWord: 0,
        Email: "",
      };
      try {
        localStorage.setItem("selectedPodcaster", JSON.stringify(payload));
        navigate("/media-player/management/bookings/create");
      } catch (e) {
        // storage may be full or unavailable in some privacy modes
        console.error("Failed to save selected podcaster to localStorage", e);
      }
    }
  };

  const handleFollowPodcaster = async () => {
    if (!podcasterDetailsRaw) {
      return;
    }

    if (!user) {
      const confirmLogin = window.confirm(
        "You need to login to follow this podcaster. Do you want to go to login page?"
      );
      if (confirmLogin) {
        navigate("/auth/login");
      }
      return;
    }

    try {
      await follow({
        PodcasterId: podcasterDetailsRaw.AccountId,
      }).unwrap();
    } catch (error) {
      dispatch(
        setError({
          message: "Failed to follow podcaster. Please try again.",
          autoClose: 5,
        })
      );
    }
  };

  const handleUnfollowPodcaster = async () => {
    if (!podcasterDetailsRaw) {
      return;
    }

    if (!user) {
      const confirmLogin = window.confirm(
        "You need to login to follow this podcaster. Do you want to go to login page?"
      );
      if (confirmLogin) {
        navigate("/auth/login");
      }
      return;
    }

    try {
      await unfollow({
        PodcasterId: podcasterDetailsRaw.AccountId,
      }).unwrap();
    } catch (error) {
      dispatch(
        setError({
          message: "Failed to follow podcaster. Please try again.",
          autoClose: 5,
        })
      );
    }
  };

  if (
    isPodcasterDetailsLoading ||
    isShowsLoading ||
    isChannelsLoading 
  ) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="font-poppins font-bold text-[#D9D9D9]">
          Finding Podcaster
        </p>
      </div>
    );
  }

  if (isNotFound) {
    return (
      <div className="w-full h-full flex flex-col gap-5 items-center justify-center">
        <LiaDizzy size={100} className="text-[#D9D9D9] animate-bounce" />
        <p className="font-poppins font-bold text-[#D9D9D9]">
          We couldn’t find the podcaster you’re looking for.
        </p>
        <LiquidButton
          onClick={() => navigate("/media-player/podcasters")}
          variant="colored"
        >
          <p>Back To Podcasters</p>
        </LiquidButton>
      </div>
    );
  } else {
    return (
      <div className="w-full flex flex-col">
        <div className="w-full flex items-center px-5 py-2">
          <div
            onClick={() => navigate(-1)}
            className="h-12 flex items-center gap-3 text-white hover:underline cursor-pointer"
          >
            <IoIosArrowBack size={20} />
            <p className="font-light font-poppins">Back</p>
          </div>
        </div>

        {/* Customer Informations with strongly blurred background image */}
        <div className="w-full h-100 flex flex-col md:flex-row px-10 relative overflow-hidden">
          {/* blurred background image (covers full area) */}
          <AutoResolveImage
            FileKey={podcasterDetailsRaw?.MainImageFileKey || ""}
            type="AccountPublicSource"
            className="absolute inset-0 w-full h-full object-cover filter blur-xl scale-110 opacity-80"
          />

          {/* dark overlay to keep foreground readable */}
          <div className="absolute inset-0 bg-black/50" />

          {/* actual content goes above the background */}
          <div className="relative z-10 w-full flex items-center gap-10">
            <div className="w-78 h-79 shrink-0 rounded-full overflow-hidden shadow-xl">
              <AutoResolveImage
                FileKey={podcasterDetailsRaw?.MainImageFileKey || ""}
                type="AccountPublicSource"
                className="w-full h-full object-cover"
              />
            </div>


            <div className="flex-1 min-w-0 flex flex-col justify-start text-white gap-3">
              <p
                className="font-bold p-0 m-0 whitespace-nowrap overflow-hidden text-ellipsis"
                style={{
                  fontSize: "clamp(32px, 5vw, 96px)",
                }}
              >
                {podcasterDetailsRaw?.Name.toLocaleUpperCase()}
              </p>
              <div className="w-full flex items-center gap-2">
                <BiSolidBadgeCheck />
                <p className="font-poppins font-bold text-gray-300">
                  {podcasterDetailsRaw?.TotalFollow.toLocaleString()} Followers
                </p>
              </div>

              <div
                className="text-sm text-[#D9D9D9] mt-2 line-clamp-4 w-2/3 overflow-ellipsis"
                dangerouslySetInnerHTML={{
                  __html: podcasterDetailsRaw?.Description || "",
                }}
              />
              <div className="w-full  flex items-center justify-start gap-5 mt-5">
                {!podcasterDetailsRaw?.IsFollowedByCurrentUser ? (
                  <LiquidButton
                    onClick={() => handleFollowPodcaster()}
                    variant="minimal"
                    disabled={isFollowing}
                  >
                    <p>{isFollowing ? "Following..." : "Follow"}</p>
                  </LiquidButton>
                ) : (
                  <LiquidButton
                    onClick={() => handleUnfollowPodcaster()}
                    variant="minimal"
                  >
                    <p>Followed</p>
                  </LiquidButton>
                )}
                {podcasterDetailsRaw?.IsBuddy && user && (
                  // <div
                  //   onClick={() => handleCreateBooking()}
                  //   className="px-5 h-[30px] bg-mystic-green text-black cursor-pointer transition-all hover:-translate-y-1 ease-in-out duration-500 flex items-center justify-center rounded-full font-bold"
                  // >
                  //   <p>Book This Podcaster</p>
                  // </div>
                  <LiquidButton
                    variant="minimal"
                    onClick={() => handleCreateBooking()}
                  >
                    <p>Book This Podcaster</p>
                  </LiquidButton>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Podcasters Channels */}
        <div className="w-full mt-20 flex flex-col gap-5 px-8">
          <p className="text-5xl font-poppins font-bold text-white">
            MY <span className="text-mystic-green">CHANNELS</span>
          </p>
          <div className="w-full">
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 4000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {channelsRaw?.ChannelList.map((channel) => (
                  <CarouselItem
                    key={channel.Id}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/4"
                  >
                    <div
                      onClick={() =>
                        navigate(`/media-player/channels/${channel.Id}`)
                      }
                      className="p-5"
                    >
                      <ChannelCard channel={channel} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          </div>
        </div>

        {/* Podcasters Shows */}
        <div className="w-full mt-5 flex flex-col">
          <p className="text-5xl font-poppins font-bold text-white m-8">
            MY <span className="text-mystic-green">SHOWS</span>
          </p>
          <div className="w-full flex flex-col gap-10">
            {showsByCategory.map((categoryGroup) => (
              <ShowsByCategoryCarousel
                key={categoryGroup.Category.Id}
                item={categoryGroup}
              />
            ))}
          </div>
        </div>

        {podcasterDetailsRaw && (
          <div className="w-full px-5 py-10 mt-20 flex flex-col bg-black/10 gap-10">
            <p className="font-bold font-poppins text-white text-5xl">
              Rating & Reviews
            </p>

            <RatingChart rating={podcasterDetailsRaw.ReviewList} />

            <div className="w-full">
              <Carousel
                opts={{
                  align: "start",
                  loop: true,
                }}
                plugins={[
                  Autoplay({
                    delay: 4000,
                  }),
                ]}
                className="w-full"
              >
                <CarouselContent>
                  {podcasterDetailsRaw.ReviewList.map((review) => (
                    <CarouselItem
                      key={review.Id}
                      className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                    >
                      <div className="p-1">
                        <ReviewCard review={review as PodcasterReviewAPI} />
                      </div>
                    </CarouselItem>
                  ))}
                </CarouselContent>
              </Carousel>
            </div>
          </div>
        )}
      </div>
    );
  }
};
export default PodcasterDetailsPage;
