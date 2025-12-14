import { useGetFollowedShowsQuery } from "@/core/services/show/show.service";
import { useGetSubscribedContentsQuery } from "@/core/services/subscription/subscription.service";
import type { ShowUI } from "@/core/types/show";
import { useEffect, useState } from "react";
import { resolveFiles } from "@/core/utils/fileResolver.util";
import ShowCard from "./components/ShowCard";
import FireLoading from "@/components/fireLoading";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";

const SubscribedShowsPage = () => {
  // STATES
  const [followedShows, setFollowedShows] = useState<ShowUI[]>([]);
  const [subscribedShows, setSubscribedShows] = useState<ShowUI[]>([]);
  const [followedSearchQuery, setFollowedSearchQuery] = useState<string>("");
  const [subscribedSearchQuery, setSubscribedSearchQuery] =
    useState<string>("");
  const [followedSelectedCategories, setFollowedSelectedCategories] = useState<
    string[]
  >([]);
  const [subscribedSelectedCategories, setSubscribedSelectedCategories] =
    useState<string[]>([]);
  const [isResolvingFollowed, setIsResolvingFollowed] =
    useState<boolean>(false);
  const [isResolvingSubscribed, setIsResolvingSubscribed] =
    useState<boolean>(false);

  // HOOKS
  const { data: followedShowsData, isFetching: isLoadingFollowedShows } =
    useGetFollowedShowsQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });
  const {
    data: subscribedContentsData,
    isFetching: isLoadingSubscribedContents,
  } = useGetSubscribedContentsQuery(undefined, {
    refetchOnMountOrArgChange: true,
    refetchOnFocus: true,
    refetchOnReconnect: true,
  });

  useEffect(() => {
    const resolveFileData = async () => {
      // Resolve followed shows independently
      if (!isLoadingFollowedShows && followedShowsData) {
        setIsResolvingFollowed(true);
        const { resolvedData: resolvedData1 } = await resolveFiles(
          followedShowsData,
          [
            {
              path: "ShowList[].MainImageFileKey",
              type: "PodcastPublic",
              output: "ShowList[].ImageUrl",
            },
            {
              path: "ShowList[].Podcaster.MainImageFileKey",
              type: "AccountPublic",
              output: "ShowList[].Podcaster.ImageUrl",
            },
            {
              path: "ShowList[].PodcastChannel.MainImageFileKey",
              type: "PodcastPublic",
              output: "ShowList[].PodcastChannel.ImageUrl",
            },
          ]
        );
        setFollowedShows(resolvedData1.ShowList as unknown as ShowUI[]);
        setIsResolvingFollowed(false);
      }

      // Resolve subscribed shows independently
      if (!isLoadingSubscribedContents && subscribedContentsData) {
        setIsResolvingSubscribed(true);
        const { resolvedData: resolvedData2 } = await resolveFiles(
          subscribedContentsData,
          [
            {
              path: "PodcastShowList[].MainImageFileKey",
              type: "PodcastPublic",
              output: "PodcastShowList[].ImageUrl",
            },
            {
              path: "PodcastShowList[].Podcaster.MainImageFileKey",
              type: "AccountPublic",
              output: "PodcastShowList[].Podcaster.ImageUrl",
            },
            {
              path: "PodcastShowList[].PodcastChannel.MainImageFileKey",
              type: "PodcastPublic",
              output: "PodcastShowList[].PodcastChannel.ImageUrl",
            },
          ]
        );
        setSubscribedShows(
          resolvedData2.PodcastShowList as unknown as ShowUI[]
        );
        setIsResolvingSubscribed(false);
      }
    };
    resolveFileData();
  }, [
    followedShowsData,
    subscribedContentsData,
    isLoadingFollowedShows,
    isLoadingSubscribedContents,
  ]);

  // Filter followed shows by search query and category
  const filteredFollowedShows = followedShows.filter((show) => {
    const matchesSearch = show.Name.toLowerCase().includes(
      followedSearchQuery.toLowerCase()
    );
    const matchesCategory =
      followedSelectedCategories.length === 0 ||
      followedSelectedCategories.includes(show.PodcastCategory.Name);
    return matchesSearch && matchesCategory;
  });

  // Get unique categories from followed shows
  const followedCategories = Array.from(
    new Set(followedShows.map((ch) => ch.PodcastCategory.Name))
  );

  // Filter subscribed shows by search query and category
  const filteredSubscribedShows = subscribedShows.filter((show) => {
    const matchesSearch = show.Name.toLowerCase().includes(
      subscribedSearchQuery.toLowerCase()
    );
    const matchesCategory =
      subscribedSelectedCategories.length === 0 ||
      subscribedSelectedCategories.includes(show.PodcastCategory.Name);
    return matchesSearch && matchesCategory;
  });

  // Get unique categories from subscribed shows
  const subscribedCategories = Array.from(
    new Set(subscribedShows.map((ch) => ch.PodcastCategory.Name))
  );

  // Toggle category selection for followed
  const toggleFollowedCategory = (category: string) => {
    setFollowedSelectedCategories((prev) =>
      prev.includes(category)
        ? prev.filter((c) => c !== category)
        : [...prev, category]
    );
  };

  // Toggle category selection for subscribed
  const toggleSubscribedCategory = (category: string) => {
    setSubscribedSelectedCategories((prev) =>
      prev.includes(category)
        ? prev.filter((c) => c !== category)
        : [...prev, category]
    );
  };

  return (
    <div className="w-full h-full flex flex-col py-10 gap-20">
      {/* Followed Shows */}
      <div className="w-full flex flex-col gap-5">
        <p className="mx-8 font-poppins text-6xl text-white font-semibold">
          <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#12c2e9] via-[#e0b0f8] to-[#f3d9db]">
            Followed
          </span>{" "}
          Shows
        </p>

        {followedShows && followedShows.length > 0 ? (
          <div className="mx-8 p-4 flex items-center justify-start gap-6 bg-white/10 border border-white/20 rounded-md shadow-lg hover:bg-white/[0.12] transition-all duration-300">
            {/* Search Query Input */}
            <input
              type="text"
              placeholder="Search by show name..."
              value={followedSearchQuery}
              onChange={(e) => setFollowedSearchQuery(e.target.value)}
              className="flex-1 px-4 py-3 bg-black/30 border border-white/20 rounded-md text-white placeholder:text-[#D9D9D9] focus:outline-none focus:bg-black/15 focus:border-white/40 transition-all duration-200 backdrop-blur-sm"
            />
            {/* Category Filter Dropdown */}
            <Popover>
              <PopoverTrigger asChild>
                <div
                  className="
                  px-5 py-3 
                  bg-gradient-to-r from-[#12c2e9]/20 via-[#e0b0f8]/20 to-[#f3d9db]/20
                  border border-white/20
                  rounded-md 
                  text-white text-sm font-semibold font-poppins
                  transition-all duration-300 ease-out 
                  hover:from-[#12c2e9]/30 hover:via-[#e0b0f8]/30 hover:to-[#f3d9db]/30
                  hover:border-white/40
                  hover:shadow-lg hover:-translate-y-0.5 
                  cursor-pointer backdrop-blur-sm
                  "
                >
                  <p>Categories: {followedSelectedCategories.length}</p>
                </div>
              </PopoverTrigger>
              <PopoverContent
                align="end"
                sideOffset={12}
                className="w-80 bg-white/10 backdrop-blur-xl border border-white/20 rounded-2xl shadow-xl"
              >
                <div className="rounded-lg flex flex-col gap-4">
                  <p className="text-white font-poppins font-semibold text-sm">
                    Filter by Categories
                  </p>
                  <div className="grid grid-cols-2 w-full gap-3">
                    {followedCategories.map((category) => (
                      <div key={category} className="flex items-center gap-2">
                        <Checkbox
                          id={`followed-category-${category}`}
                          checked={followedSelectedCategories.includes(
                            category
                          )}
                          onCheckedChange={() =>
                            toggleFollowedCategory(category)
                          }
                          className="
                            border-white/40
                            data-[state=checked]:bg-[#aee339]
                            data-[state=checked]:border-[#aee339]
                            data-[state=checked]:text-black
                          "
                        />
                        <Label
                          className="text-white/90 font-medium cursor-pointer text-sm"
                          htmlFor={`followed-category-${category}`}
                        >
                          {category}
                        </Label>
                      </div>
                    ))}
                  </div>
                </div>
              </PopoverContent>
            </Popover>
          </div>
        ) : (
          <div className="flex items-center text-start">
            <p className="mx-8 font-poppins text-white font-light">
              You have not followed any shows yet. Explore and follow your
              favorite shows to see them here!
            </p>
          </div>
        )}

        {isLoadingFollowedShows ? (
          <div className="mx-8 flex flex-col items-center justify-end h-48 p-5 relative">
            <FireLoading />
            <p className="font-poppins font-semibold text-[#D9D9D9]">
              Searching For Your Followed Shows ...
            </p>
          </div>
        ) : (
          <div className="mx-8 grid grid-cols-3 gap-6">
            {filteredFollowedShows.map((show) => (
              <ShowCard
                key={show.Id}
                show={show}
                isLoadingImage={isResolvingFollowed}
              />
            ))}
          </div>
        )}
      </div>
      {/* Subscribed Shows */}
      <div className="w-full flex flex-col gap-5">
        <p className="mx-8 font-poppins text-6xl text-white font-semibold">
          <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#74ebd5] to-[#ACB6E5]">
            Subscribed
          </span>{" "}
          Shows
        </p>

        {subscribedShows && subscribedShows.length > 0 ? (
          <div className="mx-8 p-4 flex items-center justify-start gap-6 bg-white/10 border border-white/20 rounded-md shadow-lg hover:bg-white/[0.12] transition-all duration-300">
            {/* Search Query Input */}
            <input
              type="text"
              placeholder="Search by show name..."
              value={subscribedSearchQuery}
              onChange={(e) => setSubscribedSearchQuery(e.target.value)}
              className="flex-1 px-4 py-3 bg-black/30 border border-white/20 rounded-md text-white placeholder:text-[#D9D9D9] focus:outline-none focus:bg-black/15 focus:border-white/40 transition-all duration-200 backdrop-blur-sm"
            />
            {/* Category Filter Dropdown */}
            <Popover>
              <PopoverTrigger asChild>
                <div
                  className="
                  px-5 py-3 
                  bg-gradient-to-r from-[#74ebd5]/20 to-[#ACB6E5]/20 
                  border border-white/20
                  rounded-md 
                  text-white text-sm font-semibold font-poppins
                  transition-all duration-300 ease-out 
                  hover:from-[#74ebd5]/30 hover:to-[#ACB6E5]/30 
                  hover:border-white/40
                  hover:shadow-lg hover:-translate-y-0.5 
                  cursor-pointer backdrop-blur-sm
                  "
                >
                  <p>Categories: {subscribedSelectedCategories.length}</p>
                </div>
              </PopoverTrigger>
              <PopoverContent
                align="start"
                sideOffset={12}
                className="w-80 bg-white/10 backdrop-blur-xl border border-white/20 rounded-2xl shadow-xl"
              >
                <div className="rounded-lg flex flex-col gap-4">
                  <p className="text-white font-poppins font-semibold text-sm">
                    Filter by Categories
                  </p>
                  <div className="grid grid-cols-2 w-full gap-3">
                    {subscribedCategories.map((category) => (
                      <div key={category} className="flex items-center gap-2">
                        <Checkbox
                          id={`subscribed-category-${category}`}
                          checked={subscribedSelectedCategories.includes(
                            category
                          )}
                          onCheckedChange={() =>
                            toggleSubscribedCategory(category)
                          }
                          className="
                            border-white/40
                            data-[state=checked]:bg-[#aee339]
                            data-[state=checked]:border-[#aee339]
                            data-[state=checked]:text-black
                          "
                        />
                        <Label
                          className="text-white/90 font-medium cursor-pointer text-sm"
                          htmlFor={`subscribed-category-${category}`}
                        >
                          {category}
                        </Label>
                      </div>
                    ))}
                  </div>
                </div>
              </PopoverContent>
            </Popover>
          </div>
        ) : (
          <div className="flex items-center text-start">
            <p className="mx-8 font-poppins text-white font-light">
              You have not subscribed to any shows yet. Explore and subscribe to
              your favorite shows to see them here!
            </p>
          </div>
        )}

        {isLoadingSubscribedContents ? (
          <div className="mx-8 flex flex-col items-center justify-end h-48 p-5 relative">
            <FireLoading />
            <p className="font-poppins font-semibold text-[#D9D9D9]">
              Searching For Your Subscribed Shows ...
            </p>
          </div>
        ) : (
          <div className="mx-8 grid grid-cols-3 gap-6">
            {filteredSubscribedShows.map((show) => (
              <ShowCard
                key={show.Id}
                show={show}
                isLoadingImage={isResolvingSubscribed}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default SubscribedShowsPage;
