import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import type { ChannelDetailsFromApi } from "@/core/types/channel";
import type { ShowFromAPI } from "@/core/types/show";
import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import { IoIosArrowBack } from "react-icons/io";
import { IoHeartOutline, IoHeartSharp, IoPaperPlane } from "react-icons/io5";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Check } from "lucide-react";

import { useNavigate, useParams } from "react-router-dom";
import ShowCard from "./components/ShowCard";
import {
  useGetChannelDetailsQuery,
  useGetActiveChannelSubscriptionQuery,
  useFavoriteChannelMutation,
  useUnfavoriteChannelMutation,
} from "@/core/services/channel/channel.service";
import {
  useGetCustomerRegistrationInfoFromChannelQuery,
  useSubscribePodcastSubscriptionMutation,
  useUnsubscribePodcastSubscriptionMutation,
} from "@/core/services/subscription/subscription.service";
import Loading from "@/components/loading";
import { LiaDizzy } from "react-icons/lia";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import type {
  PodcastSubscriptionCycleTypePriceType,
  SubscriptionDetails,
} from "@/core/types/subscription";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

const ACCENT = "#aee339";

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

const formatVND = (n: number) =>
  n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });

const cycleSuffix = (cycleName: string) => {
  const n = (cycleName || "").toLowerCase();
  if (n.includes("month")) return "/month";
  if (n.includes("year") || n.includes("annual")) return "/year";
  return "/cycle";
};

type ShowMaps = {
  CategoryId: number;
  Name: string;
  Shows: ShowFromAPI[];
};

const RenderSubscriptionSection = ({
  subscription,
  onOpen,
  isUserSubscribed,
  onCancel,
}: {
  subscription: SubscriptionDetails;
  onOpen: () => void;
  isUserSubscribed: boolean;
  onCancel: () => void;
}) => {
  const renderPriceOptions = (
    data?: PodcastSubscriptionCycleTypePriceType[]
  ) => {
    const list = data ? data : [];
    if (!list.length || list.length === 0) return "";
    const cheapest = list.reduce(
      (min, cur) => (cur.Price < min.Price ? cur : min),
      list[0]
    );
    return `Only from ${formatVND(cheapest.Price)}đ for ${
      cheapest.SubscriptionCycleType.Name
    }`;
  };
  if (isUserSubscribed) {
    return (
      <div className="z-20 absolute right-5 bottom-5 flex flex-col gap-2 p-3 rounded-xl">
        <LiquidButton onClick={onCancel} variant="minimal">
          <p>Cancel Subscription</p>
        </LiquidButton>
      </div>
    );
  }
  return (
    <div
      className="z-20 w-75 absolute right-5 bottom-5 flex flex-col gap-2 p-3 rounded-xl
                 bg-[rgba(174,227,57,0.85)] text-black shadow-[15px_15px_20px_#0000008c]
                 backdrop-blur-md"
    >
      <div className="flex items-center justify-between w-full">
        <p className="text-xs font-bold line-clamp-1">
          {subscription.Name ? subscription.Name.toUpperCase() : "Subscription"}
        </p>
        <p className="text-[9px] text-black/70 font-bold">Subscription</p>
      </div>
      <div
        className="text-[12px] font-semibold line-clamp-2"
        dangerouslySetInnerHTML={{ __html: subscription.Description || "" }}
      />
      <div className="text-[10px] italic w-full flex items-center justify-between mt-3">
        <p>
          {renderPriceOptions(
            subscription.PodcastSubscriptionCycleTypePriceList
          )}
        </p>
        <button
          onClick={onOpen}
          className="group w-8 h-8 p-1 bg-white text-black flex items-center justify-center
                       rounded-full shadow-[5px_5px_20px_#0000008c] hover:scale-105 transition"
          aria-label="Open subscription dialog"
        >
          <IoPaperPlane
            className="group-hover:-rotate-90 transition-all duration-700 ease-out"
            size={15}
          />
        </button>
      </div>
    </div>
  );
};

const ChannelDetailsPage = () => {
  const { id } = useParams();

  // REDUX
  const user = useSelector((state: RootState) => state.auth.user);

  // STATES
  const [channel, setChannel] = useState<
    ChannelDetailsFromApi["Channel"] | null
  >(null);
  const [shows, setShows] = useState<ShowMaps[]>([]);
  const [isSubscriptionDialogOpen, setIsSubscriptionDialogOpen] =
    useState(false);
  const [isUserSubscribed, setIsUserSubscribed] = useState(false);
  const [currentSubscription, setCurrentSubscription] =
    useState<SubscriptionDetails | null>(null);

  const [isFollowed, setIsFollowed] = useState(false);

  // HOOKS
  const dispatch = useDispatch();
  // Lấy Channel Details
  const { data: channelRaw, isFetching: isChannelDetailLoading } =
    useGetChannelDetailsQuery(
      { ChannelId: id! },
      {
        skip: !id,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  // Lấy gói subscription đang được active cho Channel này
  const {
    data: activeSubscriptionRaw,
    isLoading: isActiveSubscriptionLoading,
    refetch: refetchActiveSubscription,
  } = useGetActiveChannelSubscriptionQuery({ ChannelId: id! }, { skip: !id });

  // Lấy thông tin đăng ký của User với channel này
  // Nếu chưa đăng ký thì trả về null
  const {
    data: customerRegistrationInfo,
    isFetching: isCustomerRegistrationInfoLoading,
    refetch: refetchCustomerRegistrationInfo,
  } = useGetCustomerRegistrationInfoFromChannelQuery(
    { PodcastChannelId: id! },
    { skip: !id }
  );

  // Mutation đăng ký subscription
  const [subscribePodcastSubscription] =
    useSubscribePodcastSubscriptionMutation();

  // Mutation hủy đăng ký subscription
  const [unsubscribePodcastSubscription] =
    useUnsubscribePodcastSubscriptionMutation();

  // Mutation follow/unfollow channel
  const [favoriteChannel] = useFavoriteChannelMutation();
  const [unfavoriteChannel] = useUnfavoriteChannelMutation();

  const navigate = useNavigate();

  useEffect(() => {
    const resolveData = async () => {
      console.log("[CHANNEL DEBUG] Step 1: useEffect triggered");
      console.log("[CHANNEL DEBUG] - id:", id);
      console.log(
        "[CHANNEL DEBUG] - isChannelDetailLoading:",
        isChannelDetailLoading
      );
      console.log("[CHANNEL DEBUG] - user:", user);
      console.log(
        "[CHANNEL DEBUG] - isActiveSubscriptionLoading:",
        isActiveSubscriptionLoading
      );
      console.log(
        "[CHANNEL DEBUG] - isCustomerRegistrationInfoLoading:",
        isCustomerRegistrationInfoLoading
      );

      if (!id) {
        console.log("[CHANNEL DEBUG] No ID found, navigating back");
        navigate("/media-player/channels");
        return;
      }

      // Đợi API loading xong
      if (
        isChannelDetailLoading ||
        (user && isActiveSubscriptionLoading) ||
        (user && isCustomerRegistrationInfoLoading)
      ) {
        return;
      }

      // Check if user is subscribed
      setIsUserSubscribed(false);
      if (
        user &&
        customerRegistrationInfo &&
        activeSubscriptionRaw &&
        activeSubscriptionRaw.PodcastSubscription
      ) {
        if (
          customerRegistrationInfo.PodcastSubscriptionRegistration
            ?.PodcastSubscriptionId ===
          activeSubscriptionRaw.PodcastSubscription.Id
        ) {
          setIsUserSubscribed(true);
          console.log("[CHANNEL DEBUG] User is subscribed");
        }
      }

      if (!channelRaw) {
        console.log("[CHANNEL DEBUG] No channelRaw data, stopping");
        return;
      }

      setIsFollowed(channelRaw?.Channel.IsFavoritedByCurrentUser || false);

      // ---- 3) Lọc shows published và gom theo category ----
      const publishedShows: ShowFromAPI[] = (
        channelRaw.Channel.ShowList ?? []
      ).filter((show) => show?.CurrentStatus?.Id === 3);

      // group shows by PodcastCategory.Id
      const groupsMap = new Map<number, ShowMaps>();
      for (const s of publishedShows) {
        const cat = s?.PodcastCategory;
        if (!cat) continue;
        const catId = Number(cat.Id) || 0;
        const existing = groupsMap.get(catId);
        if (existing) {
          existing.Shows.push(s);
        } else {
          groupsMap.set(catId, {
            CategoryId: catId,
            Name: cat.Name ?? "",
            Shows: [s],
          });
        }
      }

      const groupedShows: ShowMaps[] = Array.from(groupsMap.values());

      // ---- 4) Set state & log ----
      setChannel(channelRaw.Channel as ChannelDetailsFromApi["Channel"]);
      setCurrentSubscription(
        activeSubscriptionRaw?.PodcastSubscription as SubscriptionDetails
      );
      setShows(groupedShows);
    };

    resolveData();
  }, [
    id,
    channelRaw,
    isChannelDetailLoading,
    user,
    activeSubscriptionRaw,
    isActiveSubscriptionLoading,
    customerRegistrationInfo,
    isCustomerRegistrationInfoLoading,
    navigate,
  ]);

  // FUNCTIONS
  const handleSubscribe = async (cycleTypeId: number) => {
    if (isUserSubscribed) {
      dispatch(
        setError({
          message: "You have already subcribe this channel",
          autoClose: 10,
        })
      );
      return;
    }
    if (!currentSubscription) {
      dispatch(
        setError({
          message: "Seems like this channel doesn't have any subscription!",
          autoClose: 10,
        })
      );
      return;
    }

    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to perform this action!",
          autoClose: 10,
        })
      );
      return;
    }

    try {
      // Optimistic UI update
      setIsUserSubscribed(true);
      setIsSubscriptionDialogOpen(false);

      await subscribePodcastSubscription({
        PodcastSubscriptionId: currentSubscription.Id,
        CycleTypeId: cycleTypeId,
      }).unwrap();

      // Refetch to get updated subscription data
      await refetchActiveSubscription();
      await refetchCustomerRegistrationInfo();
    } catch (error) {
      console.error("Failed to subscribe:", error);
      // Revert optimistic update on error
      setIsUserSubscribed(false);
    }
  };

  const handleCancelSubscription = async () => {
    if (!isUserSubscribed) {
      dispatch(
        setError({
          message:
            "You need to subscribe this channel first to perform this action",
          autoClose: 10,
        })
      );
      return;
    }
    if (
      !customerRegistrationInfo ||
      !customerRegistrationInfo.PodcastSubscriptionRegistration?.Id
    ) {
      dispatch(
        setError({
          message: "Cannot find the subscription that you want to cancel :(",
          autoClose: 10,
        })
      );
      return;
    }
    if (!user) {
      dispatch(
        setError({
          message: "You need to login first to perform this action!",
          autoClose: 10,
        })
      );
      return;
    }

    try {
      // Optimistic UI update
      setIsUserSubscribed(false);

      await unsubscribePodcastSubscription({
        PodcastSubscriptionRegistrationId:
          customerRegistrationInfo.PodcastSubscriptionRegistration.Id,
      }).unwrap();

      // Refetch to get updated subscription data
      await refetchActiveSubscription();
      await refetchCustomerRegistrationInfo();
    } catch (error) {
      dispatch(
        setError({
          message: error as string,
          autoClose: 10,
        })
      );
      // Revert optimistic update on error
      setIsUserSubscribed(true);
    }
  };

  const handleFollow = async (follow: boolean) => {
    setIsFollowed(follow);
    // Call API to follow/unfollow channel here
    if (follow) {
      try {
        await favoriteChannel({ PodcastChannelId: id! }).unwrap();
        // Refetch channel details to update follow status
        await refetchActiveSubscription();
        await refetchCustomerRegistrationInfo();
      } catch (error) {
        dispatch(
          setError({
            message: "Failed to follow the channel. Please try again.",
            autoClose: 10,
          })
        );
        setIsFollowed(false);
      }
    } else {
      try {
        await unfavoriteChannel({ PodcastChannelId: id! }).unwrap();
        // Refetch channel details to update follow status
        await refetchActiveSubscription();
        await refetchCustomerRegistrationInfo();
      } catch (error) {
        dispatch(
          setError({
            message: "Failed to unfollow the channel. Please try again.",
            autoClose: 10,
          })
        );
        setIsFollowed(true);
      }
    }
  };

  if (
    isChannelDetailLoading ||
    (user && isCustomerRegistrationInfoLoading) ||
    (user && isActiveSubscriptionLoading)
  ) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="font-poppins font-bold text-[#D9D9D9]">
          Loading Channel...
        </p>
      </div>
    );
  }

  if (!channel) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <LiaDizzy size={100} className="text-[#D9D9D9] animate-bounce" />
        <p className="font-poppins font-bold text-[#D9D9D9]">
          Channel Not Found...
        </p>
      </div>
    );
  }

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
      <div className="w-full h-100 flex flex-col items-center justify-center px-10 relative overflow-hidden">
        {/* blurred background image (covers full area) */}
        <AutoResolveImage
          FileKey={channel.BackgroundImageFileKey}
          type="PodcastPublicSource"
          className="absolute inset-0 w-full h-full object-cover filter blur-[2px] scale-110 opacity-80"
        />

        <div className="absolute inset-0 w-full h-full bg-black opacity-50 z-10" />

        <div className="w-full z-20 h-full flex-1 flex flex-col items-center justify-center gap-2 relative">
          <AutoResolveImage
            FileKey={channel?.MainImageFileKey}
            type="PodcastPublicSource"
            className="aspect-square w-43.75 object-cover rounded-md shadow-[10px_10px_20px_#0000008c]"
          />

          <p className="text-white text-2xl font-bold mt-5">
            {channel?.Name.toUpperCase()}
          </p>
          <div
            className="text-[#d9d9d9] text-center text-sm font-md w-1/3 overflow-hidden line-clamp-3"
            dangerouslySetInnerHTML={{
              __html: channel?.Description || "",
            }}
          />

          <div className="text-xs flex items-center justify-center gap-2 text-[#d9d9d9] font-semibold overflow-ellipsis line-clamp-1">
            <p
              onClick={() =>
                navigate(
                  `/media-player/categories/${channel.PodcastCategory.Id}`
                )
              }
              className="hover:text-mystic-green hover:underline cursor-pointer"
            >
              {channel?.PodcastCategory.Name.toUpperCase()}
            </p>{" "}
            •{" "}
            <p className="hover:text-mystic-green hover:underline cursor-pointer">
              {channel?.PodcastSubCategory.Name.toUpperCase()}
            </p>{" "}
            • <p>{channel?.ShowCount.toLocaleString()} shows</p>
          </div>

          <div className="absolute w-10 h-10 z-20 top-7 right-0 flex items-center justify-center p-2 rounded-full bg-white/20">
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

        {!isUserSubscribed ? (
          currentSubscription ? (
            <RenderSubscriptionSection
              subscription={currentSubscription as SubscriptionDetails}
              onOpen={() => setIsSubscriptionDialogOpen(true)}
              isUserSubscribed={isUserSubscribed}
              onCancel={handleCancelSubscription}
            />
          ) : (
            <div></div>
          )
        ) : (
          <div className="z-20 absolute right-5 bottom-5 flex flex-col gap-2 p-3 rounded-xl">
            <LiquidButton
              onClick={() => handleCancelSubscription()}
              variant="minimal"
            >
              <p>Cancel Subscription</p>
            </LiquidButton>
          </div>
        )}
      </div>
      <div className="w-full p-8 flex flex-col">
        {shows.map((group) => (
          <div className="w-full flex flex-col">
            <div className="w-full flex items-center justify-between">
              <p className="font-bold text-3xl text-white">
                <span className="text-mystic-green">{group.Name}</span> Shows
              </p>
            </div>
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
                  {group.Shows.map((show) => (
                    <CarouselItem
                      key={show.Id}
                      className="basis-1/2 md:basis-1/3 lg:basis-1/5"
                    >
                      <div
                        onClick={() =>
                          navigate(`/media-player/shows/${show.Id}`)
                        }
                        className="p-5"
                      >
                        <ShowCard show={show} />
                      </div>
                    </CarouselItem>
                  ))}
                </CarouselContent>
              </Carousel>
            </div>
          </div>
        ))}
      </div>
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
              <DialogDescription className="text-white/70 ">
                {/* {currentSubscription?.Description} */}
                <div dangerouslySetInnerHTML={{ __html: currentSubscription?.Description || ""}}/>
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
                  (d) => (
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
                (d) => (
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
                            (b) => (
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
                            handleSubscribe(d.SubscriptionCycleType.Id)
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

export default ChannelDetailsPage;
