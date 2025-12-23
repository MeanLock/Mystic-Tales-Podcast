import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import Autoplay from "embla-carousel-autoplay";
import YouMightLikeItCard from "./components/YouMightLikeItCardCarousel";
import { useEffect, useState } from "react";

import { GrFormNext } from "react-icons/gr";
import ShowCard from "./components/ShowCard";
import ShowCardWithCategory from "./components/ShowCardWithCategory";
import EpisodeCard from "./components/EpisodeCard";
import { useGetDiscoveryFeedQuery } from "@/core/services/feed/feed.service";

import type {
  BaseOnYourTaste,
  ContinueListening,
  HotThisWeek,
  NewReleases,
  RandomCategory,
  TalentedRookies,
  TopPodcasters,
  TopSubCategory,
} from "@/core/types/feed";

import type { ShowFromAPI } from "@/core/types/show";
import Loading from "@/components/loading";
import type { ChannelFromAPI } from "@/core/types/channel";
import ChannelCard from "./components/ChannelCard";
import PodcasterCard from "./components/PodcasterCard";
import { useNavigate } from "react-router-dom";

const DiscoveryPage = () => {
  // STATES
  const [isLoading, setIsLoading] = useState(false);

  // Base On Your Taste Data
  const [baseOnYourTaste, setBaseOnYourTaste] =
    useState<BaseOnYourTaste | null>(null);
  // New Shows Data
  const [newShows, setNewShows] = useState<NewReleases | null>(null);
  // Hot This Week Data
  const [hotThisWeek, setHotThisWeek] = useState<HotThisWeek | null>(null);
  // Top Podcasters Data
  const [topPodcasters, setTopPodcasters] = useState<TopPodcasters | null>(
    null
  );
  // Talented Rookies Data
  const [talentedRookies, setTalentedRookies] =
    useState<TalentedRookies | null>(null);
  // Top Subcategory Data
  const [topSubcategory, setTopSubcategory] = useState<TopSubCategory | null>(
    null
  );
  // Random Category Data
  const [randomCategory, setRandomCategory] = useState<RandomCategory | null>(
    null
  );
  // Continue Listening Data
  const [continueListening, setContinueListening] =
    useState<ContinueListening | null>(null);

  // HOOKS
  const navigate = useNavigate();
  // Fetch Discovery Data
  const { data: discoveryData, isFetching: isDiscoveryLoading } =
    useGetDiscoveryFeedQuery(undefined, {
      refetchOnFocus: true,
      refetchOnReconnect: true,
      refetchOnMountOrArgChange: true,
    });

  useEffect(() => {
    const resolveEachSection = () => {
      if (!discoveryData) return;
      setIsLoading(true);

      try {
        // BasedOnYourTaste
        if (discoveryData.BasedOnYourTaste) {
          setBaseOnYourTaste(discoveryData.BasedOnYourTaste);
        }

        // NewReleases
        if (discoveryData.NewReleases) {
          setNewShows(discoveryData.NewReleases);
        }

        // HotThisWeek
        if (discoveryData.HotThisWeek) {
          setHotThisWeek(discoveryData.HotThisWeek);
        }

        // TopPodcasters
        if (discoveryData.TopPodcasters) {
          setTopPodcasters(discoveryData.TopPodcasters);
        }

        // TalentedRookies
        if (discoveryData.TalentedRookies) {
          setTalentedRookies(discoveryData.TalentedRookies);
        }

        // TopSubCategory
        if (discoveryData.TopSubCategory) {
          setTopSubcategory(discoveryData.TopSubCategory);
        }

        // RandomCategory
        if (discoveryData.RandomCategory) {
          setRandomCategory(discoveryData.RandomCategory);
        }

        // ContinueListening
        if (discoveryData.ContinueListening) {
          setContinueListening(discoveryData.ContinueListening);
        }
      } catch (error) {
        console.error("Failed to resolve discovery data files:", error);
        // fallback
      } finally {
        setIsLoading(false);
      }
    };

    resolveEachSection();
  }, [discoveryData]);


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
      <div className="w-full flex flex-col gap-2">
        <p className="text-9xl font-poppins font-bold bg-linear-to-r from-[#aee339] to-[#5EFCE8] bg-clip-text text-transparent">
          Discovery
        </p>
        <p className="font-poppins text-white font-bold">
          Discover what the podcast world is talking about.
        </p>
        <p className="w-2/3 font-poppins text-[#d9d9d9]">
          Your feed highlights trending voices, standout shows, and episodes
          picked to match your taste.
        </p>
        <p className="font-poppins text-[#d9d9d9]">
          <span className="font-bold text-white">Updated constantly</span> —
          just explore and dive into what inspires you.
        </p>
      </div>
      {/* You might like it */}
      {baseOnYourTaste && baseOnYourTaste.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4 pt-2">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins text-white text-5xl font-bold">
              Base On <span className="text-mystic-green">Your Taste</span>
            </p>
          </div>
          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-mystic-green text-lg hover:underline">
              Base On What You Listened
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
                      <YouMightLikeItCard card={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Continue Listening */}
      {continueListening && continueListening.ListenSessionList.length > 0 && (
        <div className="w-full flex flex-col gap-4 py-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins text-white text-4xl font-bold">
              <span className="text-mystic-green">Continue </span>Listening
            </p>
          </div>
          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-mystic-green text-lg hover:underline">
              Continue Listening
            </p>
            <GrFormNext color="#aae339" size={25} />
          </div>

          {isDiscoveryLoading ? (
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
                    className="basis-1/2 md:basis-1/3 lg:basis-1/5"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full aspect-3/4 rounded-xl" />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          ) : (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              plugins={[
                Autoplay({
                  delay: 2000,
                }),
              ]}
              className="w-full"
            >
              <CarouselContent>
                {continueListening?.ListenSessionList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/5"
                  >
                    <div className="p-1">
                      <EpisodeCard listenSession={card as any} />
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
                      <ShowCard card={card as ShowFromAPI} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Channels This Week */}
      {hotThisWeek && hotThisWeek.ChannelList.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Hot <span className="text-mystic-green">Channels</span> This Week
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Hot</span> Channels
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
                      <ChannelCard card={card as ChannelFromAPI} />
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
                      <ShowCard card={card as ShowFromAPI} />
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
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Top</span> Podcasters Listened
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
                    className="basis-1/3 md:basis-1/3 lg:basis-1/5"
                  >
                    <div className="p-2">
                      <PodcasterCard
                        podcaster={
                          card as TopPodcasters["PodcasterList"][number]
                        }
                      />
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
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">Talented</span> Rookies
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
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">
                {topSubcategory.PodcastSubCategory.Name}
              </span>
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
                      <ShowCard card={card as ShowFromAPI} />
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
            <p onClick={() => navigate(`/media-player/categories/${randomCategory.PodcastCategory.Id}`)} className="text-sm font-bold cursor-pointer underline text-gray-300 hover:text-mystic-green">
              See all
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start cursor-pointer">
            <p className="font-poppins font-semibold text-white text-md hover:underline">
              <span className="text-mystic-green">
                {randomCategory.PodcastCategory.Name}
              </span>
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
                      <ShowCardWithCategory card={card as ShowFromAPI} />
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
