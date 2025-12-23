import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import PodcasterCard from "./components/PodcasterCard";
import Autoplay from "embla-carousel-autoplay";
import { useGetPodcastersQuery } from "@/core/services/podcasters/podcasters.service";
import Loading from "@/components/loading";

const PodcastersPage = () => {
  // STATES
  // const [popularPodcasters, setPopularPodcasters] = useState<
  //   PodcasterFromApi[] | null
  // >(null);
  // const [hotPodcasters, setHopularPodcasters] = useState<
  //   PodcasterFromApi[] | null
  // >(null);
  // const [talentedRookies, setTalentedRookies] = useState<
  //   PodcasterFromApi[] | null
  // >(null);
  // const [isFileResolving, setIsFileResolving] = useState(false);

  // HOOKS
  const { data: popularPodcasters, isFetching: isPopularPodcasterLoading } =
    useGetPodcastersQuery(
      { queryKey: "popular" },
      {
        refetchOnFocus: true,
        refetchOnReconnect: true,
        refetchOnMountOrArgChange: true,
      }
    );
  const { data: hotPodcasters, isFetching: isHotPodcastersLoading } =
    useGetPodcastersQuery(
      { queryKey: "hotRencently" },
      {
        refetchOnFocus: true,
        refetchOnReconnect: true,
        refetchOnMountOrArgChange: true,
      }
    );
  const { data: talentedRookies, isFetching: isTalentedRookiesLoading } =
    useGetPodcastersQuery(
      { queryKey: "talentedRookie" },
      {
        refetchOnFocus: true,
        refetchOnReconnect: true,
        refetchOnMountOrArgChange: true,
      }
    );

  // useEffect(() => {
  //   const resolveData = async () => {
  //     // Đợi API loading xong
  //     if (
  //       isPopularPodcasterLoading ||
  //       isHotPodcastersLoading ||
  //       isTalentedRookiesLoading
  //     ) {
  //       return;
  //     }

  //     // Resolve Popular Podcasters
  //     if (popularPodcastersRaw && popularPodcastersRaw.PodcasterList) {
  //       setPopularPodcasters(popularPodcastersRaw.PodcasterList);
  //     } else {
  //       setPopularPodcasters([]);
  //     }

  //     // Resolve Hot Podcasters
  //     if (hotPodcastersRaw && hotPodcastersRaw.PodcasterList) {
  //       setHopularPodcasters(hotPodcastersRaw.PodcasterList);
  //     } else {
  //       setHopularPodcasters([]);
  //     }

  //     // Resolve Talented Rookies
  //     if (talentedRookiesRaw && talentedRookiesRaw.PodcasterList) {
  //       setTalentedRookies(talentedRookiesRaw.PodcasterList);
  //     } else {
  //       setTalentedRookies([]);
  //     }
  //   };
  //   resolveData();
  // }, [
  //   popularPodcastersRaw,
  //   hotPodcastersRaw,
  //   talentedRookiesRaw,
  //   isPopularPodcasterLoading,
  //   isHotPodcastersLoading,
  //   isTalentedRookiesLoading,
  // ]);

  // FUNCTIONS

  if (
    isHotPodcastersLoading ||
    isPopularPodcasterLoading ||
    isTalentedRookiesLoading
  ) {
    return (
      <div className="w-full h-full flex items-center flex-col justify-center gap-5">
        <Loading />
        <p className="font-poppins text-[#D9D9D9] font-bold">
          Finding Amazing Podcasters For You...
        </p>
      </div>
    );
  }

  return (
    <div className="w-full flex flex-col relative p-8">
      <div className="w-full flex flex-col items-start justify-center mb-10 gap-2">
        <p className="text-9xl pb-4 font-poppins font-bold text-transparent bg-clip-text bg-linear-to-r from-[#FFFFFf] via-[#6DD5FA] to-[#2980B9]">
          Our Podcasters
        </p>
        <p className="font-poppins text-white font-bold">
          Meet Our Podcasters.
        </p>
        <p className="w-2/3 font-poppins text-[#d9d9d9]">
          Behind every show is a creator with a voice, a story, and a vision.
        </p>
        <p className="font-poppins text-[#d9d9d9] w-1/2">
          <span className="font-bold text-white">
            Our podcasters are storytellers, educators, entertainers, and
            experts from all over the world
          </span>{" "}
          — turning everyday ideas into conversations worth listening to.
        </p>
      </div>

      {/* Content */}
      {/* Top Podcaster */}
      {popularPodcasters && popularPodcasters.PodcasterList.length > 0 && (
        <div className="mt-5 flex flex-col gap-2">
          <div className="w-full flex flex-col gap-8">
            <div className="w-full flex items-center justify-between">
              <p className="text-5xl font-bold text-white font-poppins">
                <span className="text-mystic-green">Popular</span> Podcasters of
                all time!
              </p>
            </div>
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
                {popularPodcasters.PodcasterList.map((podcaster) => (
                  <CarouselItem
                    key={podcaster.AccountId}
                    className="basis-1/1 md:basis-1/2 lg:basis-1/3"
                  >
                    <div className="p-0">
                      <PodcasterCard podcaster={podcaster} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          </div>
        </div>
      )}

      {/* Hot Recently */}
      {hotPodcasters && hotPodcasters.PodcasterList.length > 0 && (
        <div className="mt-10 flex flex-col gap-2">
          <div className="w-full flex flex-col gap-8">
            <div className="w-full flex items-center justify-between">
              <p className="text-5xl font-bold text-white font-poppins">
                <span className="text-mystic-green">Hot</span> Podcasters this
                week
              </p>
            </div>
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
                {hotPodcasters.PodcasterList.map((podcaster) => (
                  <CarouselItem
                    key={podcaster.AccountId}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/5"
                  >
                    <div className="p-0">
                      <PodcasterCard podcaster={podcaster} />
                    </div>
                  </CarouselItem>
                ))}
              </CarouselContent>
            </Carousel>
          </div>
        </div>
      )}

      {/* Talented Rookies */}
      {talentedRookies && talentedRookies.PodcasterList.length > 0 && (
        <div className="mt-10 flex flex-col gap-2">
          <div className="w-full flex flex-col gap-8">
            <div className="w-full flex items-center justify-between">
              <p className="text-5xl font-bold text-white font-poppins">
                <span className="text-mystic-green">Newbie</span> with new vibe!
              </p>
            </div>
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
                {talentedRookies.PodcasterList.map((podcaster) => (
                  <CarouselItem
                    key={podcaster.AccountId}
                    className="basis-1/2 md:basis-1/4 lg:basis-1/5"
                  >
                    <div className="p-0">
                      <PodcasterCard podcaster={podcaster} />
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
};

export default PodcastersPage;
