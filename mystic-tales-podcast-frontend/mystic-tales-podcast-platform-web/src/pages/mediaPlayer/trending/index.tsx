import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { Skeleton } from "@/components/ui/skeleton";
import { useGetTrendingFeedQuery } from "@/core/services/feed/feed.service";
import type {
  CategoryX,
  HotChannels,
  HotPodcasters,
  HotShows,
  NewEpisodes,
  PopularChannels,
  PopularEpisodes,
  PopularPodcasters,
  PopularShows,
} from "@/core/types/feed";
import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import PodcasterCard from "./components/PodcasterCard";
import ChannelCard from "./components/ChannelCard";
import ShowCard from "./components/ShowCard";
import EpisodeCard from "./components/EpisodeCard";
import Loading from "@/components/loading";
import { useNavigate } from "react-router-dom";

const TrendingPage = () => {
  // STATES
  const [isLoading, setIsLoading] = useState<boolean>(false);

  // Data For Rendering
  // Popular Podcasters
  const [popularPodcasters, setPopularPodcasters] =
    useState<PopularPodcasters | null>(null);
  // Hot Podcasters
  const [hotPodcasters, setHotPodcasters] = useState<HotPodcasters | null>(
    null
  );
  // Popular Channels
  const [popularChannels, setPopularChannels] =
    useState<PopularChannels | null>(null);
  // Hot Channels
  const [hotChannels, setHotChannels] = useState<HotChannels | null>(null);
  // Popular Shows
  const [popularShows, setPopularShows] = useState<PopularShows | null>(null);
  // Hot Shows
  const [hotShows, setHotShows] = useState<HotShows | null>(null);
  // Popular Episodes
  const [popularEpisodes, setPopularEpisodes] =
    useState<PopularEpisodes | null>(null);
  // New Episodes
  const [newEpisodes, setNewEpisodes] = useState<NewEpisodes | null>(null);
  // Categories
  const [category1, setCategory1] = useState<CategoryX | null>(null);
  const [category2, setCategory2] = useState<CategoryX | null>(null);
  const [category3, setCategory3] = useState<CategoryX | null>(null);
  const [category4, setCategory4] = useState<CategoryX | null>(null);
  const [category5, setCategory5] = useState<CategoryX | null>(null);
  const [category6, setCategory6] = useState<CategoryX | null>(null);

  // HOOKS
  const navigate = useNavigate();

  const { data: trendingDataFromAPI, isFetching: isTrendingDataLoading } =
    useGetTrendingFeedQuery(undefined, {
      // refetchOnFocus: true,
      // refetchOnReconnect: true,
      // refetchOnMountOrArgChange: true,
    });

  useEffect(() => {
    const resolveEachSection = async () => {
      if (!trendingDataFromAPI) return;
      setIsLoading(true);

      try {
        if (
          trendingDataFromAPI.PopularPodcasters &&
          trendingDataFromAPI.PopularPodcasters.PodcasterList &&
          trendingDataFromAPI.PopularPodcasters.PodcasterList.length > 0
        ) {
          setPopularPodcasters(trendingDataFromAPI.PopularPodcasters);
        }

        if (
          trendingDataFromAPI.HotPodcasters &&
          trendingDataFromAPI.HotPodcasters.PodcasterList &&
          trendingDataFromAPI.HotPodcasters.PodcasterList.length > 0
        ) {
          setHotPodcasters(trendingDataFromAPI.HotPodcasters);
        }

        if (
          trendingDataFromAPI.PopularChannels &&
          trendingDataFromAPI.PopularChannels.ChannelList &&
          trendingDataFromAPI.PopularChannels.ChannelList.length > 0
        ) {
          setPopularChannels(trendingDataFromAPI.PopularChannels);
        }

        if (
          trendingDataFromAPI.HotChannels &&
          trendingDataFromAPI.HotChannels.ChannelList &&
          trendingDataFromAPI.HotChannels.ChannelList.length > 0
        ) {
          setHotChannels(trendingDataFromAPI.HotChannels);
        }

        if (
          trendingDataFromAPI.PopularShows &&
          trendingDataFromAPI.PopularShows.ShowList &&
          trendingDataFromAPI.PopularShows.ShowList.length > 0
        ) {
          setPopularShows(trendingDataFromAPI.PopularShows);
        }

        if (
          trendingDataFromAPI.HotShows &&
          trendingDataFromAPI.HotShows.ShowList &&
          trendingDataFromAPI.HotShows.ShowList.length > 0
        ) {
          setHotShows(trendingDataFromAPI.HotShows);
        }

        // Resolve New Episodes
        if (trendingDataFromAPI.NewEpisodes) {
          setNewEpisodes(trendingDataFromAPI.NewEpisodes);
        }

        // Resolve Popular Episodes
        if (trendingDataFromAPI.PopularEpisodes) {
          setPopularEpisodes(trendingDataFromAPI.PopularEpisodes);
        }

        if (
          trendingDataFromAPI.Category1 &&
          trendingDataFromAPI.Category1.ShowList.length > 0 &&
          trendingDataFromAPI.Category1.PodcastCategory
        ) {
          setCategory1(trendingDataFromAPI.Category1);
        }
        if (
          trendingDataFromAPI.Category2 &&
          trendingDataFromAPI.Category2.ShowList.length > 0 &&
          trendingDataFromAPI.Category2.PodcastCategory
        ) {
          setCategory2(trendingDataFromAPI.Category2);
        }
        if (
          trendingDataFromAPI.Category3 &&
          trendingDataFromAPI.Category3.ShowList.length > 0 &&
          trendingDataFromAPI.Category3.PodcastCategory
        ) {
          setCategory3(trendingDataFromAPI.Category3);
        }
        if (
          trendingDataFromAPI.Category4 &&
          trendingDataFromAPI.Category4.ShowList.length > 0 &&
          trendingDataFromAPI.Category4.PodcastCategory
        ) {
          setCategory4(trendingDataFromAPI.Category4);
        }
        if (
          trendingDataFromAPI.Category5 &&
          trendingDataFromAPI.Category5.ShowList.length > 0 &&
          trendingDataFromAPI.Category5.PodcastCategory
        ) {
          setCategory5(trendingDataFromAPI.Category5);
        }
        if (
          trendingDataFromAPI.Category6 &&
          trendingDataFromAPI.Category6.ShowList.length > 0 &&
          trendingDataFromAPI.Category6.PodcastCategory
        ) {
          setCategory6(trendingDataFromAPI.Category6);
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
  if (isTrendingDataLoading || isLoading) {
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
        <p className="text-9xl pb-4 font-poppins font-bold text-transparent bg-clip-text bg-linear-to-r from-[#DBE6F6] to-[#C5796D]">
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Podcasters
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Podcasters
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Channels
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
                    key={`${index}--${channel.Id}`}
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Channels
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
                    key={`${index}--${channel.Id}`}
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Shows
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
                    key={`${index}--${show.Id}`}
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Hot
              </span>{" "}
              Shows
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
                    key={`${index}--${show.Id}`}
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                New
              </span>{" "}
              Episodes
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
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                Popular
              </span>{" "}
              Episodes
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
      {category1 && category1.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                {category1.PodcastCategory.Name}
              </span>
            </p>
            <p
              onClick={() =>
                navigate(
                  `/media-player/categories/${category1.PodcastCategory.Id}`
                )
              }
              className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]"
            >
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
                {category1.ShowList.map((show, index) => (
                  <CarouselItem
                    key={`${index}--${show.Id}`}
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
                {category1.ShowList.map((card, index) => (
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

      {/* Category 2 */}
      {category2 && category2.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                {category2.PodcastCategory.Name}
              </span>
            </p>
            <p
              onClick={() =>
                navigate(
                  `/media-player/categories/${category2.PodcastCategory.Id}`
                )
              }
              className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]"
            >
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
                {category2.ShowList.map((show, index) => (
                  <CarouselItem
                    key={`${index}--${show.Id}`}
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
                {category2.ShowList.map((card, index) => (
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

      {/* Category 3 */}
      {category3 && category3.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                {category3.PodcastCategory.Name}
              </span>
            </p>
            <p
              onClick={() =>
                navigate(
                  `/media-player/categories/${category3.PodcastCategory.Id}`
                )
              }
              className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]"
            >
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
                {category3.ShowList.map((show, index) => (
                  <CarouselItem
                    key={`${index}--${show.Id}`}
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
                {category3.ShowList.map((card, index) => (
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

      {/* Category 4 */}
      {category4 && category4.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                {category4.PodcastCategory.Name}
              </span>
            </p>
            <p
              onClick={() =>
                navigate(
                  `/media-player/categories/${category4.PodcastCategory.Id}`
                )
              }
              className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]"
            >
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
                {category4.ShowList.map((show, index) => (
                  <CarouselItem
                    key={`${index}--${show.Id}`}
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
                {category4.ShowList.map((card, index) => (
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

      {/* Category 5 */}
      {category5 && category5.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                {category5.PodcastCategory.Name}
              </span>
            </p>
            <p
              onClick={() =>
                navigate(
                  `/media-player/categories/${category5.PodcastCategory.Id}`
                )
              }
              className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]"
            >
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
                {category5.ShowList.map((show, index) => (
                  <CarouselItem
                    key={`${index}--${show.Id}`}
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
                {category5.ShowList.map((card, index) => (
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

      {/* Category 6 */}
      {category6 && category6.ShowList.length > 0 && (
        <div className="w-full flex flex-col gap-4">
          <div className="w-full flex items-center justify-between">
            <p className="font-poppins font-bold text-3xl text-white">
              <span className="bg-linear-to-r bg-clip-text text-transparent from-[#EF3B36] to-[#FFFFFF]">
                {category6.PodcastCategory.Name}
              </span>
            </p>
            <p
              onClick={() =>
                navigate(
                  `/media-player/categories/${category6.PodcastCategory.Id}`
                )
              }
              className="font-poppins font-semibold hover:underline hover:text-mystic-green cursor-pointer text-[#d9d9d9]"
            >
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
                {category6.ShowList.map((show, index) => (
                  <CarouselItem
                    key={`${show.Id}--${index}`}
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
                {category6.ShowList.map((card, index) => (
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
    </div>
  );
};

export default TrendingPage;
