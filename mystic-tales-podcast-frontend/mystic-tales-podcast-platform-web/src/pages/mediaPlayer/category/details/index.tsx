import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { useGetCategoryFeedDataQuery } from "@/core/services/category/category.serivce";
import type { CategoryFeedDataUI } from "@/core/types/feed";
import type { FileResolveConfig } from "@/core/utils/fileResolver.util";
import { resolveFiles } from "@/core/utils/fileResolver.util";
import Autoplay from "embla-carousel-autoplay";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import ChannelCard from "./components/ChannelCard";
import ShowCard from "./components/ShowCard";
import EpisodeCard from "./components/EpisodeCard";
import { IoIosArrowRoundBack } from "react-icons/io";
import Loading from "@/components/loading";
import { Skeleton } from "@/components/ui/skeleton";

// File config cho TopChannels - array trực tiếp
const topChannelsFileConfig: FileResolveConfig[] = [
  {
    type: "PodcastPublic",
    path: "[].MainImageFileKey",
    output: "[].ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "[].BackgroundImageFileKey",
    output: "[].BackgroundImageUrl",
  },
  {
    type: "AccountPublic",
    path: "[].Podcaster.MainImageFileKey",
    output: "[].Podcaster.ImageUrl",
  },
];

// File config cho TopShows/HotShows - array trực tiếp
const showsArrayFileConfig: FileResolveConfig[] = [
  {
    type: "PodcastPublic",
    path: "[].MainImageFileKey",
    output: "[].ImageUrl",
  },
  {
    type: "AccountPublic",
    path: "[].Podcaster.MainImageFileKey",
    output: "[].Podcaster.ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "[].PodcastChannel.MainImageFileKey",
    output: "[].PodcastChannel.ImageUrl",
  },
];

// File config cho TopEpisodes - array trực tiếp
const topEpisodesFileConfig: FileResolveConfig[] = [
  {
    type: "PodcastPublic",
    path: "[].MainImageFileKey",
    output: "[].ImageUrl",
  },
  {
    type: "PodcastPublic",
    path: "[].PodcastShow.MainImageFileKey",
    output: "[].PodcastShow.ImageUrl",
  },
];

// File config cho SubCategories items (each item has ShowList array)
const subCategoryItemFileConfig: FileResolveConfig[] = [
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

const CategoryDetailsPage = () => {
  const { id } = useParams();

  // STATES
  const [categoryFeedData, setCategoryFeedData] =
    useState<CategoryFeedDataUI | null>(null);
  const [isResolvingFiles, setIsResolvingFiles] = useState(false);

  // HOOKS
  const navigate = useNavigate();
  const { data: categoryFeedDataRaw, isFetching: isCategoryFeedDataLoading } =
    useGetCategoryFeedDataQuery(
      { PodcastCategoryId: Number(id)! },
      {
        skip: !id,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  useEffect(() => {
    const resolveFile = async () => {
      if (isCategoryFeedDataLoading || !categoryFeedDataRaw) {
        return;
      }

      setIsResolvingFiles(true);
      try {
        // Resolve từng section
        const [
          topChannelsResolved,
          topShowsResolved,
          topEpisodesResolved,
          hotShowsResolved,
        ] = await Promise.all([
          resolveFiles(categoryFeedDataRaw.TopChannels, topChannelsFileConfig),
          resolveFiles(categoryFeedDataRaw.TopShows, showsArrayFileConfig),
          resolveFiles(categoryFeedDataRaw.TopEpisodes, topEpisodesFileConfig),
          resolveFiles(categoryFeedDataRaw.HotShows, showsArrayFileConfig),
        ]);

        // Resolve SubCategorySections (array of sections)
        const subCategoriesResolved = await Promise.all(
          categoryFeedDataRaw.SubCategorySections.map((section) =>
            resolveFiles(section, subCategoryItemFileConfig)
          )
        );

        // Combine tất cả resolved data
        const resolvedData: CategoryFeedDataUI = {
          PodcastCategory: categoryFeedDataRaw.PodcastCategory,
          TopChannels: topChannelsResolved.resolvedData as any,
          TopShows: topShowsResolved.resolvedData as any,
          TopEpisodes: topEpisodesResolved.resolvedData as any,
          HotShows: hotShowsResolved.resolvedData as any,
          SubCategorySections: subCategoriesResolved.map(
            (r) => r.resolvedData
          ) as any,
        };

        setCategoryFeedData(resolvedData);
      } catch (error) {
        console.error("Error resolving category feed files:", error);
        // Fallback to original data if resolve fails
        setCategoryFeedData(categoryFeedDataRaw as any);
      } finally {
        setIsResolvingFiles(false);
      }
    };
    resolveFile();
  }, [categoryFeedDataRaw, isCategoryFeedDataLoading]);

  if (isCategoryFeedDataLoading || isResolvingFiles) {
    return (
      <div className="w-full h-full flex flex-col gap-5 items-center justify-center">
        <Loading />
        <p className="text-[#D9D9D9] font-bold font-poppins">
          Loading category feed...
        </p>
      </div>
    );
  }

  if (!categoryFeedData) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        <p className="text-white font-poppins">No data available</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center gap-5 mb-20 p-8">
      <div
        onClick={() => navigate(-1)}
        className="cursor-pointer w-full gap-2 flex items-center justify-start text-white font-poppins hover:underline"
      >
        <IoIosArrowRoundBack size={20} />
        <p>Back</p>
      </div>
      <div className="w-full flex flex-col items-start justify-center gap-2 mb-5">
        <p className="text-7xl font-poppins font-bold bg-clip-text text-transparent bg-gradient-to-r from-[#abbaab] to-[#ffffff]">
          {categoryFeedData.PodcastCategory.Name}
        </p>
      </div>

      {/* Top Channels Section */}
      {categoryFeedData.TopChannels.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Top Channels
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start">
            <p className="font-poppins font-semibold text-white text-md">
              Top <span className="text-mystic-green">Channels</span>
            </p>
          </div>

          {isResolvingFiles ? (
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
                      <Skeleton className="w-full aspect-square rounded-lg" />
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
                {categoryFeedData.TopChannels.map((channel, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/5"
                  >
                    <div className="p-1">
                      <ChannelCard channel={channel} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Top Shows Section */}
      {categoryFeedData.TopShows.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Top Shows
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start">
            <p className="font-poppins font-semibold text-white text-md">
              Top <span className="text-mystic-green">Shows</span>
            </p>
          </div>

          {isResolvingFiles ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 3 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full aspect-video rounded-lg" />
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
                {categoryFeedData.TopShows.map((show, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/3"
                  >
                    <div className="p-1">
                      <ShowCard show={show} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* Top Episodes Section */}
      {categoryFeedData.TopEpisodes.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Top Episodes
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start">
            <p className="font-poppins font-semibold text-white text-md">
              Top <span className="text-mystic-green">Episodes</span>
            </p>
          </div>

          {isResolvingFiles ? (
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
                {categoryFeedData.TopEpisodes.map((episode, index) => (
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
        </div>
      )}

      {/* Hot Shows Section */}
      {categoryFeedData.HotShows.length > 0 && (
        <div className="w-full flex flex-col mt-10 gap-5">
          <div className="hidden md:inline-flex w-full items-center justify-between">
            <p className="font-poppins font-bold text-white text-2xl">
              Hot Shows
            </p>
          </div>

          <div className="md:hidden w-full flex items-center justify-start">
            <p className="font-poppins font-semibold text-white text-md">
              <span className="text-mystic-green">Hot</span> Shows
            </p>
          </div>

          {isResolvingFiles ? (
            <Carousel
              opts={{
                align: "start",
                loop: false,
              }}
              className="w-full"
            >
              <CarouselContent>
                {Array.from({ length: 4 }).map((_, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/4"
                  >
                    <div className="p-1">
                      <Skeleton className="w-full aspect-video rounded-lg" />
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
                {categoryFeedData.HotShows.map((show, index) => (
                  <CarouselItem
                    key={index}
                    className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/4"
                  >
                    <div className="p-1">
                      <ShowCard show={show} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          )}
        </div>
      )}

      {/* SubCategories Sections */}
      {categoryFeedData.SubCategorySections.map(
        (section) =>
          section.ShowList.length > 0 && (
            <div
              key={section.PodcastSubCategory.Id}
              className="w-full flex flex-col mt-10 gap-5"
            >
              <div className="hidden md:inline-flex w-full items-center justify-between">
                <p className="font-poppins font-bold text-white text-2xl">
                  {section.PodcastSubCategory.Name}
                </p>
              </div>

              <div className="md:hidden w-full flex items-center justify-start">
                <p className="font-poppins font-semibold text-white text-md">
                  {section.PodcastSubCategory.Name}
                </p>
              </div>

              {isResolvingFiles ? (
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
                          <Skeleton className="w-full aspect-video rounded-lg" />
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
                    {section.ShowList.map((show, index) => (
                      <CarouselItem
                        key={index}
                        className="basis-full sm:basis-1/2 md:basis-1/3 lg:basis-1/5"
                      >
                        <div className="p-1">
                          <ShowCard show={show} />
                        </div>
                      </CarouselItem>
                    ))}
                  </CarouselContent>
                </Carousel>
              )}
            </div>
          )
      )}
    </div>
  );
};

export default CategoryDetailsPage;
