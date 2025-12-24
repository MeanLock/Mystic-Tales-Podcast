import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import { useGetCategoryFeedDataQuery } from "@/core/services/category/category.serivce";
import Autoplay from "embla-carousel-autoplay";
import { useNavigate, useParams } from "react-router-dom";
import ChannelCard from "./components/ChannelCard";
import ShowCard from "./components/ShowCard";
import EpisodeCard from "./components/EpisodeCard";
import { IoIosArrowRoundBack } from "react-icons/io";
import Loading from "@/components/loading";

const CategoryDetailsPage = () => {
  const { id } = useParams();

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

  if (isCategoryFeedDataLoading) {
    return (
      <div className="w-full h-full flex flex-col gap-5 items-center justify-center">
        <Loading />
        <p className="text-[#D9D9D9] font-bold font-poppins">
          Loading category feed...
        </p>
      </div>
    );
  }

  if (
    !categoryFeedDataRaw ||
    (categoryFeedDataRaw.TopChannels.length === 0 &&
      categoryFeedDataRaw.TopShows.length === 0 &&
      categoryFeedDataRaw.TopEpisodes.length === 0 &&
      categoryFeedDataRaw.HotShows.length === 0 &&
      categoryFeedDataRaw.SubCategorySections.every(
        (section) => section.ShowList.length === 0
      ))
  ) {
    return (
      <div className="w-full h-full flex items-center justify-center flex-col gap-5">
        <p className="text-white font-poppins">
          This category has no data available yet.
        </p>
        <div onClick={() => navigate(-1)} className="cursor-pointer px-10 py-2 flex items-center justify-center bg-white rounded-md text-black font-bold font-poppins hover:-translate-y-0.5 hover:shadow-md transition-all duration-500 ease-out">
          <p>Back</p>
        </div>
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
        <p className="text-7xl font-poppins font-bold bg-clip-text text-transparent bg-linear-to-r from-[#abbaab] to-[#ffffff]">
          {categoryFeedDataRaw.PodcastCategory.Name}
        </p>
      </div>

      {/* Top Channels Section */}
      {categoryFeedDataRaw.TopChannels.length > 0 && (
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
              {categoryFeedDataRaw.TopChannels.map((channel, index) => (
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
        </div>
      )}

      {/* Top Shows Section */}
      {categoryFeedDataRaw.TopShows.length > 0 && (
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
              {categoryFeedDataRaw.TopShows.map((show, index) => (
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
        </div>
      )}

      {/* Top Episodes Section */}
      {categoryFeedDataRaw.TopEpisodes.length > 0 && (
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
              {categoryFeedDataRaw.TopEpisodes.map((episode, index) => (
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
        </div>
      )}

      {/* Hot Shows Section */}
      {categoryFeedDataRaw.HotShows.length > 0 && (
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
              {categoryFeedDataRaw.HotShows.map((show, index) => (
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
        </div>
      )}

      {/* SubCategories Sections */}
      {categoryFeedDataRaw.SubCategorySections.map(
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
            </div>
          )
      )}
    </div>
  );
};

export default CategoryDetailsPage;
