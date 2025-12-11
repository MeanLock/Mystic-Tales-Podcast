import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import { useGetTrendingFeedQuery } from "@/core/services/feed/feed.service";
import type {
  CategoryXUI,
  HotChannelsUI,
  HotPodcastersUI,
  HotShowsUI,
  NewEpisodes,
  NewEpisodesUI,
  PopularChannelsUI,
  PopularEpisodes,
  PopularEpisodesUI,
  PopularPodcastersUI,
  PopularShowsUI,
  TrendingDataUI,
} from "@/core/types/feed";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";
import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import PodcasterCard from "./components/PodcasterCard";
import ChannelCard from "./components/ChannelCard";
import ShowCard from "./components/ShowCard";
import EpisodeCard from "./components/EpisodeCard";
import Loading from "@/components/loading";

// File config for Podcasters sections (Popular & Hot)
const podcastersFileConfig: FileResolveConfig[] = [
  {
    type: "AccountPublic",
    path: "PodcasterList[].MainImageFileKey",
    output: "PodcasterList[].ImageUrl",
  },
];

// File config for Category sections (Category1-6)
const categoryFileConfig: FileResolveConfig[] = [
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

// File config for Channels sections (Popular & Hot)
const channelsFileConfig: FileResolveConfig[] = [
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
];

// File config for Shows sections (Popular & Hot)
const showsFileConfig: FileResolveConfig[] = [
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

// File config for Episodes sections (New & Popular)
const episodesFileConfig: FileResolveConfig[] = [
  // Episode main image
  {
    type: "PodcastPublic",
    path: "EpisodeList[].MainImageFileKey",
    output: "EpisodeList[].ImageUrl",
  },
  // Episode's Show image
  {
    type: "PodcastPublic",
    path: "EpisodeList[].PodcastShow.MainImageFileKey",
    output: "EpisodeList[].PodcastShow.ImageUrl",
  },
];


const TrendingPage = () => {
  // STATES
  const [isLoading, setIsLoading] = useState<boolean>(false);

  // Data For Rendering
  // Popular Podcasters
  const [popularPodcasters, setPopularPodcasters] =
    useState<PopularPodcastersUI | null>(null);
  // Hot Podcasters
  const [hotPodcasters, setHotPodcasters] = useState<HotPodcastersUI | null>(
    null
  );
  // Popular Channels
  const [popularChannels, setPopularChannels] =
    useState<PopularChannelsUI | null>(null);
  // Hot Channels
  const [hotChannels, setHotChannels] = useState<HotChannelsUI | null>(null);
  // Popular Shows
  const [popularShows, setPopularShows] = useState<PopularShowsUI | null>(null);
  // Hot Shows
  const [hotShows, setHotShows] = useState<HotShowsUI | null>(null);
  // Popular Episodes
  const [popularEpisodes, setPopularEpisodes] =
    useState<PopularEpisodes | null>(null);
  // New Episodes
  const [newEpisodes, setNewEpisodes] = useState<NewEpisodes | null>(null);
  // Categories
  const [category1, setCategory1] = useState<CategoryXUI | null>(null);
  const [category2, setCategory2] = useState<CategoryXUI | null>(null);
  const [category3, setCategory3] = useState<CategoryXUI | null>(null);
  const [category4, setCategory4] = useState<CategoryXUI | null>(null);
  const [category5, setCategory5] = useState<CategoryXUI | null>(null);
  const [category6, setCategory6] = useState<CategoryXUI | null>(null);

  // HOOKS
  const { data: trendingDataFromAPI, isLoading: isTrendingDataLoading } =
    useGetTrendingFeedQuery();

  useEffect(() => {
    const resolveEachSection = async () => {
      if (!trendingDataFromAPI) return;
      setIsLoading(true);

      try {
        // Resolve Popular Podcasters
        const { resolvedData } = await resolveFiles(
          trendingDataFromAPI.PopularPodcasters,
          podcastersFileConfig
        );
        const apiData = resolvedData as unknown as PopularPodcastersUI;
        setPopularPodcasters({
          PodcasterList: [...apiData.PodcasterList],
        });

        // Resolve Category1
        if (trendingDataFromAPI.Category1) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category1,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          setCategory1({
            ...apiData,
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve Hot Podcasters
        if (trendingDataFromAPI.HotPodcasters) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.HotPodcasters,
            podcastersFileConfig
          );
          const apiData = resolvedData as unknown as HotPodcastersUI;
          setHotPodcasters({
            PodcasterList: [...apiData.PodcasterList],
          });
        }

        // Resolve Category2
        if (trendingDataFromAPI.Category2) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category2,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          setCategory2({
            ...apiData,
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve Popular Channels
        if (trendingDataFromAPI.PopularChannels) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.PopularChannels,
            channelsFileConfig
          );
          const apiData = resolvedData as unknown as PopularChannelsUI;
          setPopularChannels({
            ChannelList: [...apiData.ChannelList],
          });
        }

        // Resolve Category3
        if (trendingDataFromAPI.Category3) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category3,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          setCategory3({
            ...apiData,
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve Hot Channels
        if (trendingDataFromAPI.HotChannels) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.HotChannels,
            channelsFileConfig
          );
          const apiData = resolvedData as unknown as HotChannelsUI;
          setHotChannels({
            ChannelList: [...apiData.ChannelList],
          });
        }

        // Resolve Category4
        if (trendingDataFromAPI.Category4) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category4,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          setCategory4({
            ...apiData,
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve Popular Shows
        if (trendingDataFromAPI.PopularShows) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.PopularShows,
            showsFileConfig
          );
          const apiData = resolvedData as unknown as PopularShowsUI;
          setPopularShows({
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve Category5
        if (trendingDataFromAPI.Category5) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category5,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          setCategory5({
            ...apiData,
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve Hot Shows
        if (trendingDataFromAPI.HotShows) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.HotShows,
            showsFileConfig
          );
          const apiData = resolvedData as unknown as HotShowsUI;
          setHotShows({
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve Category6
        if (trendingDataFromAPI.Category6) {
          const { resolvedData } = await resolveFiles(
            trendingDataFromAPI.Category6,
            categoryFileConfig
          );
          const apiData = resolvedData as unknown as CategoryXUI;
          setCategory6({
            ...apiData,
            ShowList: [...apiData.ShowList],
          });
        }

        // Resolve New Episodes
        if (trendingDataFromAPI.NewEpisodes) {
          setNewEpisodes(trendingDataFromAPI.NewEpisodes);
        }

        // Resolve Popular Episodes
        if (trendingDataFromAPI.PopularEpisodes) {
          setPopularEpisodes(trendingDataFromAPI.PopularEpisodes);
        }

        console.log("✅ All trending sections resolved successfully");
      } catch (error) {
        console.error("❌ Error resolving trending data:", error);
      } finally {
        setIsLoading(false);
      }
    };

    void resolveEachSection();
  }, [trendingDataFromAPI]);

  // FUNCTIONS
  if (isTrendingDataLoading) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="font-poppins text-[#D9D9D9] font-bold">
          Loading Trending Data...
        </p>
      </div>
    );
  }
  return (
    <div
      className="
      flex flex-col items-center gap-10 mb-20 p-8
    "
    >
      <div className="w-full flex flex-col items-start justify-center mb-10 gap-2">
        <p className="text-9xl pb-4 font-poppins font-bold text-transparent bg-clip-text bg-gradient-to-r from-[#DBE6F6] to-[#C5796D]">
          Trending
        </p>
        <p className="font-poppins text-white font-bold">
          Stay in tune with the podcast community.
        </p>
        <p className="w-2/3 font-poppins text-[#d9d9d9]">
          Trending highlights everything that’s gaining attention: rising
          podcasters, popular channels, standout shows, and episodes that are
          making waves.
        </p>
        <p className="font-poppins text-[#d9d9d9]">
          <span className="font-bold text-white">Updated constantly</span> —
          just explore and dive into what inspires you.
        </p>
      </div>

      {/* Popular Podcasters */}
      {popularPodcasters && popularPodcasters.PodcasterList.length > 0 && (
        <div className="w-full flex flex-col gap-4 mb-10">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Podcasters
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
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
                {popularPodcasters.PodcasterList.map((podcaster, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">
                        {podcaster.FullName}
                      </p>
                      {/* <Skeleton className="w-8/12 h-3 rounded-xs" /> */}
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
                {popularPodcasters.PodcasterList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <PodcasterCard podcaster={card} />
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Podcasters */}
      {hotPodcasters && hotPodcasters.PodcasterList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Podcasters
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
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
                {hotPodcasters.PodcasterList.map((podcaster, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">
                        {podcaster.FullName}
                      </p>
                      {/* <Skeleton className="w-8/12 h-3 rounded-xs" /> */}
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
                {hotPodcasters.PodcasterList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <PodcasterCard podcaster={card} />
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Popular Channels */}
      {popularChannels && popularChannels.ChannelList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Channels
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full py-5"
            >
              <CarouselContent>
                {popularChannels.ChannelList.map((channel, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-md mb-2" />
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
              className="w-full py-5"
            >
              <CarouselContent>
                {popularChannels.ChannelList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/4"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <ChannelCard channel={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Channels */}
      {hotChannels && hotChannels.ChannelList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Channels
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
          </div>

          {isLoading ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full py-5"
            >
              <CarouselContent>
                {hotChannels.ChannelList.map((channel, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-md" />
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
              className="w-full py-5"
            >
              <CarouselContent>
                {hotChannels.ChannelList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-3 flex flex-col items-center">
                      <ChannelCard channel={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Popular Shows */}
      {popularShows && popularShows.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Shows
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
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
                {popularShows.ShowList.map((show, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-video rounded-md mb-2" />
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
                {popularShows.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="flex items-center justify-center py-2 px-1">
                      <ShowCard show={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Hot Shows */}
      {hotShows && hotShows.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Shows
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
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
                {hotShows.ShowList.map((show, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-video rounded-md mb-2" />
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
                {hotShows.ShowList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <ShowCard show={card} />
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* New Episodes */}
      {newEpisodes && newEpisodes.EpisodeList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                New
              </span>{" "}
              Episodes
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
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
                {newEpisodes.EpisodeList.map((episode, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">{episode.Name}</p>
                      {/* <Skeleton className="w-8/12 h-3 rounded-xs" /> */}
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
                {newEpisodes.EpisodeList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <EpisodeCard episode={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Popular Episodes */}
      {popularEpisodes && popularEpisodes.EpisodeList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-gradient-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Episodes
            </p>
            <p className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]">
              See all
            </p>
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
                {popularEpisodes.EpisodeList.map((episode, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <Skeleton className="w-full h-full aspect-square rounded-full mb-2" />
                      <p className="text-white font-poppins">{episode.Name}</p>
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
                {popularEpisodes.EpisodeList.map((card, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-1/2 md:basis-1/5 lg:basis-1/5"
                  >
                    <div className="p-1 flex flex-col items-center">
                      <EpisodeCard episode={card} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Category 1 */}
    </div>
  );
};

export default TrendingPage;
