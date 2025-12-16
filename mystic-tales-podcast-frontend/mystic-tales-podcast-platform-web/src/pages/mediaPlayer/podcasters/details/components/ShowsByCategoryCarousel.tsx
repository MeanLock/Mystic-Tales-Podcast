import {
  Carousel,
  CarouselContent,
  CarouselItem,
} from "@/components/ui/carousel";
import type { PodcastCategory } from "@/core/types/podcastCategory";
import type { ShowFromAPI } from "@/core/types/show";
import Autoplay from "embla-carousel-autoplay";
import { useNavigate } from "react-router-dom";
import ShowCard from "./ShowCard";

interface Props {
  item: {
    Category: PodcastCategory;
    ShowList: ShowFromAPI[];
  };
}

const ShowsByCategoryCarousel = ({ item }: Props) => {
  const navigate = useNavigate();
  return (
    <div className="w-full flex flex-col gap-5 px-8">
      <p className="text-3xl font-bold text-mystic-green">
        {item.Category.Name}
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
            {item.ShowList.map((show) => (
              <CarouselItem
                key={show.Id}
                className="basis-1/2 md:basis-1/3 lg:basis-1/3"
              >
                <div
                  onClick={() => navigate(`/media-player/shows/${show.Id}`)}
                  className="p-2"
                >
                  <ShowCard show={show} />
                </div>
              </CarouselItem>
            ))}
          </CarouselContent>
        </Carousel>
      </div>
    </div>
  );
};

export default ShowsByCategoryCarousel;
