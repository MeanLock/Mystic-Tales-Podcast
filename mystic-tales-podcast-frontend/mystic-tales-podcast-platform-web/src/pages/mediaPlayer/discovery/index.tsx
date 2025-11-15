/* eslint-disable @typescript-eslint/no-unused-vars */

import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import Autoplay from "embla-carousel-autoplay";
import YouMightLikeItCard from "./components/YouMightLikeItCardCarousel";
import { useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { useEffect, useState } from "react";

import { GrFormNext } from "react-icons/gr";
import ShowCard from "./components/ShowCard";
import ShowCardWithRating from "./components/ShowCardWithRating";
import ShowCardWithCategory from "./components/ShowCardWithCategory";
import { ca } from "zod/v4/locales";
import EpisodeCard from "./components/EpisodeCard";
import { useGetDiscoveryFeedQuery } from "@/core/services/feed/feed.service";
import { useNavigate } from "react-router-dom";
import type {
  BaseOnYourTaste,
  BaseOnYourTasteUI,
  DiscoveryDataFileResolved,
  HotThisWeek,
  HotThisWeekUI,
  NewReleases,
  NewReleasesUI,
  RandomCategory,
  RandomCategoryUI,
  TalentedRookies,
  TalentedRookiesUI,
  TopPodcasters,
  TopPodcastersUI,
  TopSubCategory,
  TopSubCategoryUI,
} from "@/core/types/feed";
import type { FileResolveConfig } from "@/core/utils/fileResolver.util";
import { resolveFiles } from "@/core/utils/fileResolver.util";
import type { ShowUI } from "@/core/types/show";
import Loading from "@/components/loading";
import type { ChannelUI } from "@/core/types/channel";
import ChannelCard from "./components/ChannelCard";

// File config cho từng section
const baseOnYourTasteFileConfig: FileResolveConfig[] = [
  // Show main image
  {
    type: "PodcastPublic",
    path: "ShowList[].MainImageFileKey",
    output: "ShowList[].ImageUrl",
  },
  // Podcaster avatar
  {
    type: "AccountPublic",
    path: "ShowList[].Podcaster.MainImageFileKey",
    output: "ShowList[].Podcaster.ImageUrl",
  },
  // Channel image
  {
    type: "PodcastPublic",
    path: "ShowList[].PodcastChannel.MainImageFileKey",
    output: "ShowList[].PodcastChannel.ImageUrl",
  },
];

const newReleasesFileConfig: FileResolveConfig[] = [
  ...baseOnYourTasteFileConfig,
];

const hotThisWeekFileConfig: FileResolveConfig[] = [
  // Channel main image & background
  {
    type: "PodcastPublic",
    path: "ChannelList[].MainImageFileKey",
    output: "ChannelList[].ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "ChannelList[].BackgroundImageFileKey",
    output: "ChannelList[].BackgroundImageUrl",
  },
  // Channel podcaster avatar
  {
    type: "AccountPublic",
    path: "ChannelList[].Podcaster.MainImageFileKey",
    output: "ChannelList[].Podcaster.ImageUrl",
  },
  // Show (reuse logic show)
  {
    type: "PodcastPublic",
    path: "ShowList[].MainImageFileKey",
    output: "ShowList[].ImageUrl",
  },
  {
    type: "AccountPublic",
    path: "ShowList[].Podcaster.MainImageFileKey",
    output: "ShowList[].Podcaster.ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "ShowList[].PodcastChannel.MainImageFileKey",
    output: "ShowList[].PodcastChannel.ImageUrl",
  },
];

const topSubCategoryFileConfig: FileResolveConfig[] = [
  {
    type: "PodcastPublic",
    path: "ShowList[].MainImageFileKey",
    output: "ShowList[].ImageUrl",
  },
  {
    type: "AccountPublic",
    path: "ShowList[].Podcaster.MainImageFileKey",
    output: "ShowList[].Podcaster.ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "ShowList[].PodcastChannel.MainImageFileKey",
    output: "ShowList[].PodcastChannel.ImageUrl",
  },
];

const topPodcastersFileConfig: FileResolveConfig[] = [
  {
    type: "AccountPublic",
    path: "PodcasterList[].MainImageFileKey",
    output: "PodcasterList[].ImageUrl",
  },
];

const talentedRookiesFileConfig: FileResolveConfig[] = [
  {
    type: "AccountPublic",
    path: "PodcasterList[].MainImageFileKey",
    output: "PodcasterList[].ImageUrl",
  },
];

const randomCategoryFileConfig: FileResolveConfig[] = [
  {
    type: "PodcastPublic",
    path: "ShowList[].MainImageFileKey",
    output: "ShowList[].ImageUrl",
  },
  {
    type: "AccountPublic",
    path: "ShowList[].Podcaster.MainImageFileKey",
    output: "ShowList[].Podcaster.ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "ShowList[].PodcastChannel.MainImageFileKey",
    output: "ShowList[].PodcastChannel.ImageUrl",
  },
];

const DiscoveryPage = () => {
  // STATES
  const [isLoading, setIsLoading] = useState(false);

  // Base On Your Taste Data
  const [baseOnYourTaste, setBaseOnYourTaste] = useState<
    BaseOnYourTasteUI | BaseOnYourTaste | null
  >(null);
  // New Shows Data
  const [newShows, setNewShows] = useState<NewReleasesUI | NewReleases | null>(
    null
  );
  // Hot This Week Data
  const [hotThisWeek, setHotThisWeek] = useState<
    HotThisWeekUI | HotThisWeek | null
  >(null);
  // Top Podcasters Data
  const [topPodcasters, setTopPodcasters] = useState<
    TopPodcastersUI | TopPodcasters | null
  >(null);
  // Talented Rookies Data
  const [talentedRookies, setTalentedRookies] = useState<
    TalentedRookiesUI | TalentedRookies | null
  >(null);
  // Top Subcategory Data
  const [topSubcategory, setTopSubcategory] = useState<
    TopSubCategoryUI | TopSubCategory | null
  >(null);
  // Random Category Data
  const [randomCategory, setRandomCategory] = useState<
    RandomCategoryUI | RandomCategory | null
  >(null);

  // HOOKS
  const navigate = useNavigate();

  // Fetch Discovery Data
  const { data: discoveryData, isLoading: isDiscoveryLoading } =
    useGetDiscoveryFeedQuery();

  useEffect(() => {
    const resolveEachSection = async () => {
      if (!discoveryData) return;
      setIsLoading(true);

      try {
        // BasedOnYourTaste
        if (discoveryData.BasedOnYourTaste) {
          const { resolvedData } = await resolveFiles<BaseOnYourTaste>(
            discoveryData.BasedOnYourTaste,
            baseOnYourTasteFileConfig
          );
          setBaseOnYourTaste(resolvedData as unknown as BaseOnYourTasteUI);
        }

        // NewReleases
        if (discoveryData.NewReleases) {
          const { resolvedData } = await resolveFiles<NewReleases>(
            discoveryData.NewReleases,
            newReleasesFileConfig
          );
          setNewShows(resolvedData as unknown as NewReleasesUI);
        }

        // HotThisWeek
        if (discoveryData.HotThisWeek) {
          const { resolvedData } = await resolveFiles<HotThisWeek>(
            discoveryData.HotThisWeek,
            hotThisWeekFileConfig
          );
          setHotThisWeek(resolvedData as unknown as HotThisWeekUI);
        }

        // TopPodcasters
        if (discoveryData.TopPodcasters) {
          const { resolvedData } = await resolveFiles<TopPodcasters>(
            discoveryData.TopPodcasters,
            topPodcastersFileConfig
          );
          setTopPodcasters(resolvedData as unknown as TopPodcastersUI);
        }

        // TalentedRookies
        if (discoveryData.TalentedRookies) {
          const { resolvedData } = await resolveFiles<TalentedRookies>(
            discoveryData.TalentedRookies,
            talentedRookiesFileConfig
          );
          setTalentedRookies(resolvedData as unknown as TalentedRookiesUI);
        }

        // TopSubCategory
        if (discoveryData.TopSubCategory) {
          const { resolvedData } = await resolveFiles<TopSubCategory>(
            discoveryData.TopSubCategory,
            topSubCategoryFileConfig
          );
          setTopSubcategory(resolvedData as unknown as TopSubCategoryUI);
        }

        // RandomCategory
        if (discoveryData.RandomCategory) {
          const { resolvedData } = await resolveFiles<RandomCategory>(
            discoveryData.RandomCategory,
            randomCategoryFileConfig
          );
          setRandomCategory(resolvedData as unknown as RandomCategoryUI);
        }
      } catch (error) {
        console.error("Failed to resolve discovery data files:", error);
        // fallback
        setBaseOnYourTaste(discoveryData.BasedOnYourTaste);
        setNewShows(discoveryData.NewReleases);
        setHotThisWeek(discoveryData.HotThisWeek);
        setTopPodcasters(discoveryData.TopPodcasters);
        setTalentedRookies(discoveryData.TalentedRookies);
        setTopSubcategory(discoveryData.TopSubCategory);
        setRandomCategory(discoveryData.RandomCategory);
      } finally {
        setIsLoading(false);
      }
    };

    void resolveEachSection();
  }, [discoveryData]);

  // FUNCTIONS
  const checkFileResolved = () => {
    console.log("Resolved Discovery Data:", hotThisWeek);
    console.log(baseOnYourTaste?.ShowList[0]);
  };

  if (isDiscoveryLoading) {
    return (
      <div className="w-full flex flex-col h-full items-center justify-center gap-5 mb-20 p-8">
        <Loading />
        <p className="font-poppins font-bold text-[#d9d9d9]">
          Find Your Contents...
        </p>
      </div>
    );
  }

  return (
    <div
      className="
      flex flex-col items-center gap-5 mb-20 p-8
    "
    >
      <div className="w-full flex items-center">
        <p className="text-9xl font-poppins font-bold bg-gradient-to-r from-[#aee339] to-[#5EFCE8] bg-clip-text text-transparent">
          Discovery
        </p>
      </div>
      {/* You might like it */}
      {baseOnYourTaste && baseOnYourTaste.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4 pt-2">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins text-white text-5xl font-bold">
              Base On <span className="text-mystic-green">Your Taste</span>
            </p>
            <p
              onClick={() => checkFileResolved()}
              className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green"
            >
              See all
            </p>
          </div>
          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-mystic-green text-lg hover:underline">
              You Might Like It
            </p>
            <GrFormNext color="#aae339" size={25} />
          </div>

          {isDiscoveryLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 5 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/4"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 2000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {baseOnYourTaste?.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/4"
                  >
                    <div className="p-1">
                      <YouMightLikeItCard card={card as ShowUI} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* New Shows */}
      {newShows && newShows.ShowList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              <span className="text-mystic-green">New</span> Shows
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">New</span> Shows
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {newShows.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <ShowCard card={card as ShowUI} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* New Episodes */}
      {/* <div className="w-full flex flex-col mt-10 gap-5">
        <div className="hidden md:inline-flex w-full items-center justify-between">
          <p className="font-poppins font-bold text-white text-2xl">
            New Episodes
          </p>
          <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
            See all
          </p>
        </div>

        <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
          <p className="font-poppins font-semibold text-white text-md hover:underline">
            <span className="text-mystic-green">New</span> Episodes
          </p>
          <GrFormNext color="#fff" size={25} />
        </div>

        {isLoading ? (
          <Carousel
            opts={{
              align: "start",
              loop: false,
            }}
            className="w-full"
          >
            <CarouselContent>
              {Array.from({ length: 5 }).map((_, index) => (
                <CarouselItem
                  key={index}
                  className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/5"
                >
                  <div className="p-1">
                    <Skeleton className="w-full aspect-[3/4] rounded-lg" />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        ) : (
          <Carousel
            opts={{
              align: "start",
              loop: true,
            }}
            plugins={[
              Autoplay({
                delay: 3000,
              }),
            ]}
            className="w-full"
          >
            <CarouselContent>
              {newEpisodes.map((episode, index) => (
                <CarouselItem
                  key={index}
                  className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/5"
                >
                  <div className="p-1">
                    <EpisodeCard episode={episode} />
                  </div>
                </CarouselItem>
              ))}
            </CarouselContent>
          </Carousel>
        )}
      </div> */}

      {/* Hot Channels This Week */}
      {hotThisWeek && hotThisWeek.ChannelList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Hot <span className="text-mystic-green">Channels</span> This Week
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Highly Rated</span> Shows
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
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
                {hotThisWeek.ChannelList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <ChannelCard card={card as ChannelUI} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Shows This Week */}
      {hotThisWeek && hotThisWeek.ShowList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Hot <span className="text-mystic-green">Shows</span> This Week
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Highly Rated</span> Shows
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
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
                {hotThisWeek.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <ShowCard card={card as ShowUI} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Top Podcasters */}
      {topPodcasters && topPodcasters.PodcasterList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Top <span className="text-mystic-green">Podcasters</span>
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Base On</span> What You Have
              Listened
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {topPodcasters.PodcasterList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <p>{card.FullName}</p>
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Talented Rookies */}
      {talentedRookies && talentedRookies.PodcasterList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              <span className="text-mystic-green">Breakout</span> Rookies
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Base On</span> What You Have
              Listened
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {talentedRookies.PodcasterList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <p>{card.FullName}</p>
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Top SubCategories */}
      {topSubcategory && topSubcategory.ShowList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              <span className="text-mystic-green">
                {topSubcategory.PodcastSubCategory.Name}
              </span>
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Base On</span> What You Have
              Listened
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {topSubcategory.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <ShowCard card={card as ShowUI} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Random Category */}
      {randomCategory && randomCategory.ShowList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              <span className="text-mystic-green">
                {randomCategory.PodcastCategory.Name}
              </span>
            </p>
            <p className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Base On</span> What You Have
              Listened
            </p>
            <GrFormNext color="#fff" size={25} />
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 6 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full h-full aspect-square rounded-lg mb-2" />
                      <Skeleton className="w-11/12 h-5 rounded-xs mb-2" />
                      <Skeleton className="w-8/12 h-3 rounded-xs" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: true,
              }}
              plugins={[
                Autoplay({
                  delay: 3000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {randomCategory.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/3 md:basis-1/4 lg:basis-1/6"
                  >
                    <div className="p-1">
                      <ShowCardWithCategory card={card as ShowUI} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}
    </div>
  );
};

export default DiscoveryPage;
